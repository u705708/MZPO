using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MZPO.Processors
{
    public class CorpReportProcessor
    {
        #region Definition
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly SheetsService _service;
        private readonly long _dateFrom;
        private readonly long _dateTo;
        private readonly BaseRepository<Lead> leadRepo;
        private readonly BaseRepository<Company> compRepo;
        protected readonly CancellationToken _token;
        private readonly string SpreadsheetId;

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

        private readonly List<(int, string)> managers = new List<(int, string)>
        {
            (2375116, "Киреева Светлана")//,
            //(2375122, "Васина Елена"),
            //(2375131, "Алферова Лилия"),
            //(2884132, "Ирина Сорокина"),
            //(6028753, "Алена Федосова"),
            //(6630727, "Елена Зубатых")//,
            //(3770773, "Шталева Лидия"),
            //(6200629, "Харшиладзе Леван"),
            //(6346882, "Мусихина Юлия")
        };

        private List<Request> requestContainer;
        #endregion

        #region Supplementary methods
        private void PrepareSheets()
        {
            requestContainer = new List<Request>();
            var spreadsheet = _service.Spreadsheets.Get(SpreadsheetId).Execute();
            foreach (var s in spreadsheet.Sheets)
            {
                Console.WriteLine($"{s.Properties.Index}, {s.Properties.Title}");
                if (s.Properties.Index == 0) continue;
                requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = s.Properties.SheetId } });
            }

            foreach (var m in managers)
            {
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
            }

            var batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = requestContainer;

            _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).Execute();
        }
        #endregion

        #region Realization
        public async void Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove("0");
                return;
            }

            PrepareSheets();

            foreach (var m in managers)
            {
                if (_token.IsCancellationRequested) return;

                var allLeads = leadRepo.GetByCriteria($"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[responsible_user_id]={m.Item1}");

                var leads = allLeads.Where(x =>
                    (x.custom_fields_values != null) &&
                    (x.custom_fields_values.Any(y => y.field_id == 118675)) &&
                    ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value >= _dateFrom) &&
                    ((long)x.custom_fields_values.FirstOrDefault(y => y.field_id == 118675).values[0].value <= _dateTo)
                    );

                var valueRange = new ValueRange();
                valueRange.Values = new List<IList<object>>();

                foreach (var l in leads)
                {
                    if (_token.IsCancellationRequested) return;

                    string A = "";
                    string B = "";
                    int C;
                    int D;
                    int E;
                    string F = "";
                    string G = "";
                    string H = "";
                    int I;
                    int J;
                    int K;

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
                    D = E / C;
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
                    K = E * J / 100;
                    #endregion

                    valueRange.Values.Add( new List<object>() { A, B, C, D, E, F, G, H, I, J, K } );
                }

                #region Finals
                var appendRequest = _service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, $"{m.Item2}!A:K");
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                await appendRequest.ExecuteAsync();

                //worksheet.Cells[$"A{row}"].Value = "Итого:";
                //worksheet.Cells[$"E{row}"].Formula = $"SUM(E2:E{row - 1})";
                //worksheet.Cells[$"K{row}"].Formula = $"SUM(K2:K{row - 1})";
                #endregion

                GC.Collect();
            }

            _processQueue.Remove("report_corp");
        }
        #endregion
    }
}