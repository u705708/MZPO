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
    internal class CorpReportProcessor : AbstractReportProcessor, IReportProcessor
    {
        #region Definition
        /// <summary>
        /// Формирует отчёт по продажам для корпоративного отдела.
        /// </summary>
        internal CorpReportProcessor(AmoAccount acc, TaskList processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token) 
        {
        }
        #endregion

        #region Supplementary methods
        private async Task PrepareSheets()
        {
            #region Retrieving spreadsheet
            List<Request> requestContainer = new();
            var spreadsheet = _service.Spreadsheets.Get(_spreadsheetId).Execute();
            #endregion

            #region Deleting existing sheets except first
            foreach (var s in spreadsheet.Sheets)
            {
                if (s.Properties.Index == 0) continue;
                requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = s.Properties.SheetId } });
            }
            #endregion

            #region Creating CellFormat for alignment
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

            var leftAlignment = new CellFormat()
            {
                TextFormat = new TextFormat()
                {
                    Bold = true,
                    FontSize = 11
                },
                HorizontalAlignment = "LEFT",
                VerticalAlignment = "MIDDLE"
            };
            #endregion

            foreach (var m in managersCorp)
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
                                ColumnCount = 11,
                                FrozenRowCount = 1
                            },
                            Title = m.Item2,
                            SheetId = m.Item1
                        }
                    }
                });
                #endregion

                #region Adding header
                requestContainer.Add(new Request()
                {
                    UpdateCells = new UpdateCellsRequest()
                    {

                        Fields = "*",
                        Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = m.Item1 },
                        Rows = new List<RowData>() { new RowData() { Values = new List<CellData>(){
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Оганизация"} },
                            new CellData(){ UserEnteredFormat = leftAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Назначение платежа"} },
                            new CellData(){ UserEnteredFormat = leftAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Кол-во человек"} },
                            new CellData(){ UserEnteredFormat = leftAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Стоимость, руб."} },
                            new CellData(){ UserEnteredFormat = leftAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Сумма, руб."} },
                            new CellData(){ UserEnteredFormat = leftAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Дата прихода"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Расчет"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Исполнитель"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Номер сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "% сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Вознаграждение"} }
                            } }
                        }
                    }
                });
                #endregion

                #region Adjusting column width
                var width = new List<int>() { 370, 95, 60, 84, 93, 107, 89, 107, 115, 79, 131 };
                int i = 0;

                foreach (var c in width)
                {
                    requestContainer.Add(new Request()
                    {
                        UpdateDimensionProperties = new UpdateDimensionPropertiesRequest()
                        {
                            Fields = "PixelSize",
                            Range = new DimensionRange() { SheetId = m.Item1, Dimension = "COLUMNS", StartIndex = i, EndIndex = i + 1 },
                            Properties = new DimensionProperties() { PixelSize = c }
                        }
                    });
                    i++;
                }
                #endregion
            }

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }

        private static CellData[] GetCellData(string A, string B, int C, string D, int E, string F, string G, string H, int I, int J, string K)
        {
            return new []{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = A},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = B},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = C},
                    UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = D},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = E},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = F},
                    UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "DATE" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = G},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = H},
                    UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = I},
                    UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = J},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER" }} },
                new CellData(){
                    UserEnteredValue = new ExtendedValue() { FormulaValue = K},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" } } }
            };
        }

        private static CellData[] GetLastRowCellData()
        {
            return new []{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = "Итого:"},
                    UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "RIGHT", TextFormat = new TextFormat(){ Bold = true } } },
                new CellData(),
                new CellData(),
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=AVERAGE(D2:INDIRECT(""R[-1]C[0]"", FALSE))"},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" }, TextFormat = new TextFormat(){ Bold = true } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=SUM(E2:INDIRECT(""R[-1]C[0]"", FALSE))"},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" }, TextFormat = new TextFormat(){ Bold = true } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=AVERAGE(E2:INDIRECT(""R[-1]C[-1]"", FALSE))"},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "Средняя # ### ###.00" }, TextFormat = new TextFormat(){ Bold = true } } },
                new CellData(),
                new CellData(),
                new CellData(),
                new CellData(),
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=SUM(K2:INDIRECT(""R[-1]C[0]"", FALSE))"},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" }, TextFormat = new TextFormat(){ Bold = true } } }
            };
        }

        private static Request GetProcessedLeadRequest(Lead lead, int sheetId, IAmoRepo<Company> compRepo)
        {
            #region Оганизация
            string A = "";
            if (lead._embedded.companies.Any())
                A = compRepo.GetById(lead._embedded.companies.FirstOrDefault().id).name;
            #endregion

            #region Назначение платежа
            string B = lead.GetCFStringValue(118509);
            #endregion

            #region Кол-во человек
            int C = lead.GetCFIntValue(611005);
            if (C == 0) C++;
            #endregion

            #region Сумма
            int E = (int)lead.price;
            #endregion

            #region Стоимость
            string D = @"=INDIRECT(""R[0]C[1]"", FALSE)/INDIRECT(""R[0]C[-1]"", FALSE)";
            #endregion

            #region Дата прихода
            long payment_date_unix;
            if (lead.custom_fields_values.Any(x => x.field_id == 118675))
                payment_date_unix = (long)lead.custom_fields_values.FirstOrDefault(x => x.field_id == 118675).values[0].value;
            else
                payment_date_unix = 0;
            string F = DateTimeOffset.FromUnixTimeSeconds(payment_date_unix).UtcDateTime.AddHours(3).ToShortDateString();
            #endregion

            #region Расчет
            string G = lead.GetCFStringValue(118545);
            #endregion

            #region Исполнитель
            string H = lead.GetCFStringValue(162301);
            #endregion

            #region Номер сделки
            int I = lead.id;
            #endregion

            #region % сделки
            int J = lead.GetCFIntValue(613663);
            #endregion

            #region Вознаграждение
            string K = @"=INDIRECT(""R[0]C[-6]"", FALSE)*INDIRECT(""R[0]C[-1]"", FALSE)/100";
            #endregion

            return GetRowRequest(sheetId, GetCellData(A, B, C, D, E, F, G, H, I, J, K));
        }

        private async Task ProcessManager((int, string) manager)
        {
            #region Preparing
            _processQueue.AddSubTask(_taskId, $"{_taskId}_{manager.Item2}", $"CorpReport: new leads");

            var allLeads = _leadRepo.GetByCriteria($"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[responsible_user_id]={manager.Item1}");

            var leads = allLeads.Where(x =>
                (x.custom_fields_values is not null) &&
                (x.custom_fields_values.Any(y => y.field_id == 118675)) &&
                ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value >= _dateFrom) &&
                ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value <= _dateTo)
                ).ToList();
            #endregion

            #region Processing
            List<Request> requestContainer = new();

            int i = 0;

            Parallel.ForEach(
                leads,
                new ParallelOptions { MaxDegreeOfParallelism = 12 },
                l => {
                    i++;
                    var request = GetProcessedLeadRequest(l, manager.Item1, _compRepo);
                    requestContainer.Add(request);
                    if (i % 25 == 0)
                        GC.Collect();
                });
            #endregion

            requestContainer.Add(GetRowRequest(manager.Item1, GetLastRowCellData()));

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);

            _processQueue.Remove($"{_taskId}_{manager.Item2}");
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

            await PrepareSheets();

            List<Task> tasks = new();

            foreach (var manager in managersCorp)
            {
                if (_token.IsCancellationRequested) break;

                await ProcessManager(manager);
            }

            _processQueue.Remove(_taskId);
        }
        #endregion
    }
}