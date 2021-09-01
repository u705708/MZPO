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
    internal class SuccessLeadsCallsProcessor : AbstractReportProcessor, IReportProcessor
    {
        private readonly (int, int) dataRange;
        private readonly object _locker;

        internal SuccessLeadsCallsProcessor(AmoAccount acc, ProcessQueue processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
            _locker = new();
        }

        private class RespComparer : IEqualityComparer<(int, int)>
        {
            public bool Equals((int, int) x, (int, int) y)
            {
                return x.Item1 == y.Item1;
            }

            public int GetHashCode((int, int) t)
            {
                int hashProductCode = t.Item1;

                return hashProductCode;
            }
        }

        private static string GetRetManager(int id)
        {
            List<(int, string)> managersRet = new()
            {
                (2375107, "Кристина Гребенникова"),
                (2375143, "Екатерина Белоусова"),
                (2976226, "Вера Гладкова"),
                (3835801, "Наталья Кубышина"),
                (6158035, "Анастасия Матюк"),
                (2375152, "Карен Оганисян"),
                (3813670, "Федорова Александра"),
                (6102562, "Валерия Лукьянова"),
                (6929800, "Саида Исмаилова"),
                (7358368, "Лидия Ковш"),
                (6102574, "Отдел сопровождения")
            };

            if (managersRet.Any(x => x.Item1 == id))
                return managersRet.First(x => x.Item1 == id).Item2;
            return id.ToString();
        }

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
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Менеджер"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Звонков по успешным сделкам"} },
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 144, 300 };
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

        private async Task PrepareSheets()
        {
            List<Request> requestContainer = new();

            #region Retrieving spreadsheet
            var spreadsheet = _service.Spreadsheets.Get(_spreadsheetId).Execute();
            #endregion

            #region Adding temp sheet
            requestContainer.Add(new Request()
            {
                AddSheet = new AddSheetRequest()
                {
                    Properties = new SheetProperties()
                    {
                        GridProperties = new GridProperties()
                        {
                            ColumnCount = 2,
                            FrozenRowCount = 1
                        },
                        Title = "_temp",
                        SheetId = 31337
                    }
                }
            });
            #endregion

            #region Deleting existing sheets except temp
            foreach (var s in spreadsheet.Sheets)
            {
                if (s.Properties.SheetId == 31337) continue;
                requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = s.Properties.SheetId } });
            }
            #endregion

            #region Prepare First Sheet
            requestContainer.Add(new Request()
            {
                AddSheet = new AddSheetRequest()
                {
                    Properties = new SheetProperties()
                    {
                        GridProperties = new GridProperties()
                        {
                            RowCount = 11,
                            ColumnCount = 2,
                            FrozenRowCount = 1
                        },
                        Title = DateTime.UtcNow.AddHours(3).AddDays(-1).ToShortDateString(),
                        SheetId = 0,
                        Index = 0
                    }
                }
            });

            requestContainer.Add(new Request()
            {
                UpdateCells = new UpdateCellsRequest()
                {
                    Fields = "*",
                    Range = new GridRange()
                    {
                        SheetId = 0,
                    }
                }
            });

            requestContainer.AddRange(GetHeaderRequests(0));
            #endregion

            #region Delete temp sheet
            requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = 31337 } });
            #endregion

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }

        private static CellData[] GetCellData(string A, int B)
        {
            return new[]{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = A},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = B},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER" } } },
            };
        }

        public override async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_taskId);
                return;
            }

            await PrepareSheets();

            var contRepo = _acc.GetRepo<Contact>();
            var leadRepo = _acc.GetRepo<Lead>();

            var events = leadRepo.GetEventsByCriteria($"filter[created_at][from]={_dateFrom}&filter[created_at][to]={_dateTo}&filter[type]=outgoing_call");

            var contactCallers = events.Select(x => ((int)x.entity_id, (int)x.created_by)).Distinct(new RespComparer()).ToDictionary(x => x.Item1, y => GetRetManager(y.Item2));

            var contacts = contRepo.BulkGetById(contactCallers.Select(x => x.Key));

            List<int> successContacts = new();

            Parallel.ForEach(
                contacts,
                new ParallelOptions { MaxDegreeOfParallelism = 6 },
                c =>
                {
                    if (c._embedded is null ||
                        c._embedded.leads is null ||
                        !c._embedded.leads.Any())
                        return;

                    var leads = leadRepo.BulkGetById(c._embedded.leads.Select(x => x.id));

                    if (leads.Any(x => x.pipeline_id == 3198184 &&
                                       x.status_id == 142))
                    {
                        lock (_locker) successContacts.Add((int)c.id);
                    }
                });


            var result = successContacts.Select(x => (x, contactCallers[x])).GroupBy(x => x.Item2).Select(x => new { resp = x.Key, count = x.Count() });

            List<Request> requestContainer = new();

            foreach (var l in result.Where(x => managersRet.Any(y => y.Item2 == x.resp)))
                requestContainer.Add(GetRowRequest(0, GetCellData(l.resp, l.count)));

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);

            _processQueue.Remove(_taskId);
        }
    }
}