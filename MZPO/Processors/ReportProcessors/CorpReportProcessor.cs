using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Processors
{
    public class CorpReportProcessor : IProcessor
    {
        #region Definition
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly SheetsService _service;
        private readonly string SpreadsheetId;
        private readonly long _dateFrom;
        private readonly long _dateTo;
        private readonly IAmoRepo<Lead> leadRepo;
        private readonly IAmoRepo<Company> compRepo;
        protected readonly CancellationToken _token;

        public CorpReportProcessor(AmoAccount acc, TaskList processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, CancellationToken token)
        {
            _acc = acc;
            _processQueue = processQueue;
            _service = gSheets.GetService();
            SpreadsheetId = spreadsheetId;
            _dateFrom = dateFrom;
            _dateTo = dateTo;
            _token = token;
            leadRepo = _acc.GetRepo<Lead>();
            compRepo = _acc.GetRepo<Company>();
        }

        private readonly List<(int, string)> managers = new List<(int, string)>
        {
            (2375116, "Киреева Светлана"),
            (2375122, "Васина Елена"),
            (2375131, "Алферова Лилия"),
            (2884132, "Ирина Сорокина"),
            (6028753, "Алена Федосова"),
            (6630727, "Елена Зубатых"),
            (6697522, "Наталья Филатова"),
            //(3770773, "Шталева Лидия"),
            //(6200629, "Харшиладзе Леван"),
            //(6346882, "Мусихина Юлия")
        };
        #endregion

        #region Supplementary methods
        private void PrepareSheets()
        {
            #region Retrieving spreadsheet
            List<Request> requestContainer = new();
            var spreadsheet = _service.Spreadsheets.Get(SpreadsheetId).Execute();
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

            foreach (var m in managers)
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

            #region Executing request
            UpdateSheetsAsync(requestContainer);
            #endregion
        }

        private Request GetRowRequest(int sheetId, string A, string B, int C, string D, int E, string F, string G, string H, int I, int J, string K)
        {
            #region Prepare data
            var rows = new List<RowData>
            {
                new RowData()
                {
                    Values = new List<CellData>(){
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
                    }
                }
            };
            #endregion

            return new Request()
            {
                AppendCells = new AppendCellsRequest()
                {
                    Fields = '*',
                    Rows = rows,
                    SheetId = sheetId
                }
            };
        }

        private Request GetLastRowRequest(int sheetId)
        {
            #region Prepare data
            var rows = new List<RowData>
            {
                new RowData()
                {
                    Values = new List<CellData>(){
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = "Итого:"},
                             UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "RIGHT", TextFormat = new TextFormat(){ Bold = true } } },
                         new CellData(),
                         new CellData(),
                         new CellData(),
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=SUM(E2:INDIRECT(""R[-1]C[0]"", FALSE))"},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" }, TextFormat = new TextFormat(){ Bold = true } } },
                         new CellData(),
                         new CellData(),
                         new CellData(),
                         new CellData(),
                         new CellData(),
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=SUM(K2:INDIRECT(""R[-1]C[0]"", FALSE))"},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" }, TextFormat = new TextFormat(){ Bold = true } } }
                }
                }
            };
            #endregion

            return new Request()
            {
                AppendCells = new AppendCellsRequest()
                {
                    Fields = '*',
                    Rows = rows,
                    SheetId = sheetId
                }
            };
        }

        private Request GetProcessedLeadRequest(Lead lead, int sheetId)
        {
            #region Field init
            string A = "";
            string B = "";
            int C;
            string D;
            int E;
            string F = "";
            string G = "";
            string H = "";
            int I;
            int J;
            string K;
            #endregion

            #region Оганизация
            if (lead._embedded.companies.Any())
                A = compRepo.GetById(lead._embedded.companies.FirstOrDefault().id).name;
            #endregion

            #region Назначение платежа
            if (lead.custom_fields_values.Any(x => x.field_id == 118509))
                B = (string)lead.custom_fields_values.FirstOrDefault(x => x.field_id == 118509).values[0].value;
            #endregion

            #region Кол-во человек
            int students = 1;
            if (lead.custom_fields_values.Any(x => x.field_id == 611005))
                if (!int.TryParse((string)lead.custom_fields_values.FirstOrDefault(x => x.field_id == 611005).values[0].value, out students)) students = 1;
            C = students;
            #endregion

            #region Сумма
            E = (int)lead.price;
            #endregion

            #region Стоимость
            D = @"=INDIRECT(""R[0]C[1]"", FALSE)/INDIRECT(""R[0]C[-1]"", FALSE)";
            #endregion

            #region Дата прихода
            long payment_date_unix;
            if (lead.custom_fields_values.Any(x => x.field_id == 118675))
                payment_date_unix = (long)lead.custom_fields_values.FirstOrDefault(x => x.field_id == 118675).values[0].value;
            else
                payment_date_unix = 0;
            F = DateTimeOffset.FromUnixTimeSeconds(payment_date_unix).UtcDateTime.AddHours(3).ToShortDateString();
            #endregion

            #region Расчет
            if (lead.custom_fields_values.Any(x => x.field_id == 118545))
                G = (string)lead.custom_fields_values.FirstOrDefault(x => x.field_id == 118545).values[0].value;
            #endregion

            #region Испольнитель
            if (lead.custom_fields_values.Any(x => x.field_id == 162301))
                H = (string)lead.custom_fields_values.FirstOrDefault(x => x.field_id == 162301).values[0].value;
            #endregion

            #region Номер сделки
            I = lead.id;
            #endregion

            #region % сделки
            int percent = 0;
            if (lead.custom_fields_values.Any(x => x.field_id == 613663))
                if (!int.TryParse((string)lead.custom_fields_values.FirstOrDefault(x => x.field_id == 613663).values[0].value, out percent)) percent = 0;
            J = percent;
            #endregion

            #region Вознаграждение
            K = @"=INDIRECT(""R[0]C[-6]"", FALSE)*INDIRECT(""R[0]C[-1]"", FALSE)/100";
            #endregion

            return GetRowRequest(sheetId, A, B, C, D, E, F, G, H, I, J, K);
        }

        private void ProcessManager((int, string) manager)
        {
            #region Preparing
            _processQueue.AddSubTask("report_corp", $"report_corp_{manager.Item2}", $"CorpReport: new leads");

            var allLeads = leadRepo.GetByCriteria($"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[responsible_user_id]={manager.Item1}");

            if (allLeads is null) return;

            var leads = allLeads.Where(x =>
                (x.custom_fields_values is not null) &&
                (x.custom_fields_values.Any(y => y.field_id == 118675)) &&
                ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value >= _dateFrom) &&
                ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value <= _dateTo)
                );

            List<Request> requestContainer = new();
            #endregion

            #region Processing
            _processQueue.UpdateTaskName($"report_corp_{manager.Item2}", $"CorpReport: total leads {leads.Count()}");
            foreach (var l in leads)
            {
                if (_token.IsCancellationRequested) break;
                requestContainer.Add(GetProcessedLeadRequest(l, manager.Item1));
            }
            #endregion

            #region Finalization
            requestContainer.Add(GetLastRowRequest(manager.Item1));

            UpdateSheetsAsync(requestContainer);

            _processQueue.Remove($"report_corp_{manager.Item2}");
            #endregion
        }

        private async void UpdateSheetsAsync(List<Request> requestContainer)
        {
            if (requestContainer.Any())
            {
                var batchRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = requestContainer
                };

                await _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).ExecuteAsync();
            }
        }
        #endregion

        #region Realization
        public void Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove("report_corp");
                return;
            }

            Log.Add("Started corporate report.");

            PrepareSheets();

            List<Task> tasks = new();

            foreach (var manager in managers)
            {
                if (_token.IsCancellationRequested) break;

                var m = manager;
                tasks.Add(Task.Run(() => ProcessManager(m)));
            }

            Task.WhenAll(tasks).Wait();

            Log.Add("Finished corporate report.");

            _processQueue.Remove("report_corp");
        }
        #endregion
    }
}