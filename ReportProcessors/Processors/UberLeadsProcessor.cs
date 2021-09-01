using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.ReportProcessors
{
    internal class UberLeadsProcessor : AbstractReportProcessor, IReportProcessor
    {
        #region Definition
        /// <summary>
        /// Формирует отчёт для отдела розницы, собирает показатели с начала месяца, сравнивает со среднемесячными показателями за аналогичный период.
        /// </summary>
        internal UberLeadsProcessor(AmoAccount acc, ProcessQueue processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
        }

        private readonly Dictionary<string, CellFormat> columnsFormat = new()
        {
            { "A", new CellFormat() { NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "########" } } },
            { "B", new CellFormat() { NumberFormat = new NumberFormat() { Type = "DATE" } } },
            { "C", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
        };
        #endregion

        #region Supplementary methods
        private static List<Request> GetHeaderRequests(int? sheetId)
        {
            List<Request> requestContainer = new();

            #region Creating CellFormat for header
            var centerAlignment = new CellFormat()
            {
                TextFormat = new TextFormat()
                {
                    Bold = true,
                    FontSize = 11
                },
                HorizontalAlignment = "CENTER",
                VerticalAlignment = "MIDDLE"
            };
            #endregion

            #region Adding header
            requestContainer.Add(new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {

                    Fields = "*",
                    Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = sheetId },
                    Rows = new List<RowData>() { new RowData() { Values = new List<CellData>(){
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Сделка"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Дата распределения"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Ответственный"} },
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 84, 168, 154 };
            int i = 0;

            foreach (var c in width)
            {
                requestContainer.Add(new Request()
                {
                    UpdateDimensionProperties = new UpdateDimensionPropertiesRequest()
                    {
                        Fields = "PixelSize",
                        Range = new DimensionRange() { SheetId = sheetId, Dimension = "COLUMNS", StartIndex = i, EndIndex = i + 1 },
                        Properties = new DimensionProperties() { PixelSize = c }
                    }
                });
                i++;
            }
            #endregion

            return requestContainer;
        }

        private CellData[] GetCellData(int A, DateTime B, string C)
        {
            return new[]{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = $@"=HYPERLINK(""https://mzpoeducationsale.amocrm.ru/leads/detail/{A}"", ""{A}"")" },
                    UserEnteredFormat = columnsFormat["A"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = $"{B.Day}/{B.Month}/{B.Year}" },
                    UserEnteredFormat = columnsFormat["B"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = C },
                    UserEnteredFormat = columnsFormat["C"] },
            };
        }

        private async Task PrepareSheets()
        {
            List<Request> requestContainer = new();

            #region Retrieving spreadsheet
            var spreadsheet = _service.Spreadsheets.Get(_spreadsheetId).Execute();
            #endregion

            #region Deleting existing sheets except first
            foreach (var s in spreadsheet.Sheets)
            {
                if (s.Properties.SheetId == 0) continue;
                requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = s.Properties.SheetId } });
            }
            #endregion

            foreach (var m in managersRet)
            {
                #region Adding sheet
                requestContainer.Add(new Request()
                {
                    AddSheet = new AddSheetRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            GridProperties = new GridProperties()
                            {
                                ColumnCount = columnsFormat.Count,
                                FrozenRowCount = 1
                            },
                            Title = m.Item2,
                            SheetId = m.Item1
                        }
                    }
                });
                #endregion

                requestContainer.AddRange(GetHeaderRequests(m.Item1));
            }

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }

        private static IEnumerable<int> GetUberLeadIds(DateTime startDate, DateTime endDate, IAmoRepo<Lead> leadRepo)
        {
            var startPeriod = startDate;
            var endPeriod = startPeriod.AddDays(1).AddSeconds(-1);

            IEnumerable<Lead> leads = leadRepo.GetByCriteria($"filter[created_at][from]={((DateTimeOffset)startPeriod).ToUnixTimeSeconds()}&filter[created_at][to]={((DateTimeOffset)endPeriod).ToUnixTimeSeconds()}");

            while (startPeriod < endDate)
            {
                startPeriod = startPeriod.AddHours(3).AddDays(1).AddHours(-3);
                endPeriod = endPeriod.AddSeconds(1).AddHours(3).AddDays(1).AddHours(-3).AddSeconds(-1);

                leads = leads.Concat(leadRepo.GetByCriteria($"filter[created_at][from]={((DateTimeOffset)startPeriod).ToUnixTimeSeconds()}&filter[created_at][to]={((DateTimeOffset)endPeriod).ToUnixTimeSeconds()}"));
            }

            return leads.Where(x => x.custom_fields_values is not null)
                        .Where(x => x.custom_fields_values.Any(y => y.field_id == 644287))
                        .Select(x => x.id);
        }

        private static List<(int, long, int)> ProcessUberLeads(IEnumerable<int> uberLeadIds, IAmoRepo<Lead> leadRepo)
        {
            List<(int, long, int)> entries = new();
            object locker = new();

            int i = 0;

            Parallel.ForEach(
                uberLeadIds,
                new ParallelOptions { MaxDegreeOfParallelism = 8 },
                l =>
                {
                    var events = leadRepo.GetEntityEvents(l);

                    var uberEventEntry = events.Where(e => e.type == "entity_responsible_changed")
                                               //.Where(e => e.created_by == 0)
                                               .Where(e => e.value_before is not null)
                                               .Where(e => e.value_before.Any(x => x.responsible_user is not null))
                                               .Where(e => e.value_before.Any(x => x.responsible_user.id == 2576764))
                                               .Where(e => e.value_after is not null)
                                               .Where(e => e.value_after.Any(x => x.responsible_user is not null))
                                               .OrderBy(e => e.created_at)
                                               .Select(e => ((int)e.entity_id, (long)e.created_at, (int)e.value_after.First(x => x.responsible_user is not null).responsible_user.id))
                                               .ToList();

                    if (uberEventEntry.Any())
                        lock (locker) entries.Add(uberEventEntry.First());

                    lock (locker) i++;
                    if (i % 55 == 0) GC.Collect();

                });

            return entries;
        }
        #endregion

        #region Realization
        public override async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_taskId);
                return;
            }
            try
            {
                DateTime startDate = DateTimeOffset.FromUnixTimeSeconds(_dateFrom).UtcDateTime.AddHours(3);
                DateTime endDate = DateTimeOffset.FromUnixTimeSeconds(_dateTo).UtcDateTime.AddHours(3);

                string dates = $"{startDate.ToShortDateString()} - {endDate.ToShortDateString()}";

                _processQueue.UpdateTaskName($"{_taskId}", $"Uber: {dates}");

                await PrepareSheets();

                var uberLeadIds = GetUberLeadIds(startDate, endDate, _leadRepo);

                var entries = ProcessUberLeads(uberLeadIds, _leadRepo);

                List<Request> requestContainer = new();

                foreach (var manager in managersRet)
                    foreach (var e in entries.Where(x => x.Item3 == manager.Item1))
                        requestContainer.Add(GetRowRequest(manager.Item1, GetCellData(e.Item1, DateTimeOffset.FromUnixTimeSeconds(e.Item2).UtcDateTime.AddHours(3), manager.Item2)));

                await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
            }
            catch
            {

            }
            finally
            {
                _processQueue.Remove(_taskId);
            }
        }
        #endregion
    }
}