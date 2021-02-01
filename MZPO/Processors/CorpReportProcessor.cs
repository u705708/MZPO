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
        private readonly BaseRepository<Lead> leadRepo;
        private readonly BaseRepository<Company> compRepo;
        protected readonly CancellationToken _token;

        public CorpReportProcessor(AmoAccount acc, TaskList processQueue, GSheets gSheets, string spreadsheetId, CancellationToken token, long dateFrom, long dateTo)
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

        private List<Request> requestContainer;
        private readonly List<(int, string)> managers = new List<(int, string)>
        {
            (2375116, "Киреева Светлана"),
            (2375122, "Васина Елена"),
            (2375131, "Алферова Лилия"),
            (2884132, "Ирина Сорокина"),
            (6028753, "Алена Федосова"),
            (6630727, "Елена Зубатых")//,
            //(3770773, "Шталева Лидия"),
            //(6200629, "Харшиладзе Леван"),
            //(6346882, "Мусихина Юлия")
        };
        #endregion

        #region Supplementary methods
        private void PrepareSheets()
        {
            #region Retrieving spreadsheet
            requestContainer = new List<Request>();
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
            var batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = requestContainer;

            _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).Execute();
            #endregion
        }

        private void PrepareRow(int sheetId, string A, string B, int C, string D, int E, string F, string G, string H, int I, int J, string K)
        {
            #region Prepare data
            var rows = new List<RowData>();
            rows.Add(new RowData()
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
            });
            #endregion

            #region Add request
            requestContainer.Add(new Request()
            {
                AppendCells = new AppendCellsRequest()
                {
                    Fields = '*',
                    Rows = rows,
                    SheetId = sheetId
                }
            });
            #endregion
        }

        private void LastRow(int sheetId)
        {
            #region Prepare data
            var rows = new List<RowData>();
            rows.Add(new RowData()
            {
                Values = new List<CellData>(){
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = "Итого:"},
                             UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "RIGHT", TextFormat = new TextFormat(){ Bold = true } } },
                         new CellData(),
                         new CellData(),
                         new CellData(),
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=SUM(E2:E{requestContainer.Count + 1})"},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" }, TextFormat = new TextFormat(){ Bold = true } } },
                         new CellData(),
                         new CellData(),
                         new CellData(),
                         new CellData(),
                         new CellData(),
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=SUM(K2:K{requestContainer.Count + 1})"},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00" }, TextFormat = new TextFormat(){ Bold = true } } }
                }
            });
            #endregion

            #region Add request
            requestContainer.Add(new Request()
            {
                AppendCells = new AppendCellsRequest()
                {
                    Fields = '*',
                    Rows = rows,
                    SheetId = sheetId
                }
            });
            #endregion
        }

        private void ProcessLead(Lead l, int sheetId)
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
            if (l._embedded.companies.Any())
                A = compRepo.GetById(l._embedded.companies.FirstOrDefault().id).name;
            #endregion

            #region Назначение платежа
            if (l.custom_fields_values.Any(x => x.field_id == 118509))
                B = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118509).values[0].value;
            #endregion

            #region Кол-во человек
            int students;
            if (l.custom_fields_values.Any(x => x.field_id == 611005))
                Int32.TryParse((string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 611005).values[0].value, out students);
            else
                students = 1;
            C = students;
            #endregion

            #region Сумма
            E = (int)l.price;
            #endregion

            #region Стоимость
            D = $"=E{requestContainer.Count + 2}/C{requestContainer.Count + 2}";
            #endregion

            #region Дата прихода
            long payment_date_unix;
            if (l.custom_fields_values.Any(x => x.field_id == 118675))
                payment_date_unix = (long)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118675).values[0].value;
            else
                payment_date_unix = 0;
            F = DateTimeOffset.FromUnixTimeSeconds(payment_date_unix).UtcDateTime.AddHours(3).ToShortDateString();
            #endregion

            #region Расчет
            if (l.custom_fields_values.Any(x => x.field_id == 118545))
                G = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 118545).values[0].value;
            #endregion

            #region Испольнитель
            if (l.custom_fields_values.Any(x => x.field_id == 162301))
                H = (string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 162301).values[0].value;
            #endregion

            #region Номер сделки
            I = l.id;
            #endregion

            #region % сделки
            int percent;
            if (l.custom_fields_values.Any(x => x.field_id == 613663))
                Int32.TryParse((string)l.custom_fields_values.FirstOrDefault(x => x.field_id == 613663).values[0].value, out percent);
            else
                percent = 0;
            J = percent;
            #endregion

            #region Вознаграждение
            K = $"=E{requestContainer.Count + 2}*J{requestContainer.Count + 2}/100";
            #endregion

            PrepareRow(sheetId, A, B, C, D, E, F, G, H, I, J, K);
        }

        private Task ProcessManager((int, string) m)
        {
            #region Preparing
            _processQueue.UpdateTaskName("report_corp", $"CorpReport: {m.Item2}");

            var allLeads = leadRepo.GetByCriteria($"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[responsible_user_id]={m.Item1}");

            if (allLeads is null) return Task.CompletedTask;

            var leads = allLeads.Where(x =>
                (x.custom_fields_values is { }) &&
                (x.custom_fields_values.Any(y => y.field_id == 118675)) &&
                ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value >= _dateFrom) &&
                ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value <= _dateTo)
                );

            requestContainer = new List<Request>();
            #endregion

            #region Processing
            _processQueue.UpdateTaskName("report_corp", $"CorpReport: {m.Item2}, total leads {leads.Count()}");
            foreach (var l in leads)
            {
                if (_token.IsCancellationRequested) break;
                ProcessLead(l, m.Item1);
            }
            #endregion

            #region Finalization
            LastRow(m.Item1);

            var batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = requestContainer;

            return _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).ExecuteAsync();
            #endregion
        }
        #endregion

        #region Realization
        public async void Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove("report_corp");
                return;
            }

            Log.Add("Started corporate report.");

            PrepareSheets();

            foreach (var m in managers)
            {
                if (_token.IsCancellationRequested) break;

                await ProcessManager(m);

                GC.Collect();
            }

            Log.Add("Finished corporate report.");

            _processQueue.Remove("report_corp");
        }
        #endregion
    }
}