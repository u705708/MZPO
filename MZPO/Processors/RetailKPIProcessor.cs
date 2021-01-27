using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MZPO.Processors
{
    public class RetailKPIProcessor : IProcessor
    {
        #region Definition
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly SheetsService _service;
        private readonly string SpreadsheetId;
        private readonly BaseRepository<Lead> leadRepo;
        private readonly BaseRepository<Company> compRepo;
        private readonly BaseRepository<Contact> contRepo;
        protected readonly CancellationToken _token;

        public RetailKPIProcessor(AmoAccount acc, GSheets gSheets, string spreadsheetId, TaskList processQueue, CancellationToken token)
        {
            _acc = acc;
            _processQueue = processQueue;
            _token = token;
            _service = gSheets.GetService();
            SpreadsheetId = spreadsheetId;
            leadRepo = _acc.GetRepo<Lead>();
            compRepo = _acc.GetRepo<Company>();
            contRepo = _acc.GetRepo<Contact>();
        }

        private List<Request> requestContainer;
        private readonly List<(int, string)> managers = new List<(int, string)>
        {
            (2375107, "Кристина Гребенникова"),
            (2375143, "Екатерина Белоусова"),
            (2976226, "Вера Гладкова"),
            (3835801, "Наталья Кубышина"),
            (6158035, "Анастасия Матюк"),
            (2375152, "Карен Оганисян"),
            (3813670, "Федорова Александра"),
            (6102562, "Валерия Лукьянова"),
            (6410290, "Вероника Бармина")
        };

        private readonly List<(int, int)> dataRanges = new List<(int, int)>
        {
            //(1601499600,1604177999),
            //(1604178000,1606769999),
            //(1606770000,1609448399),
            //(1609448400,1612126799)
            (1610917200,1611522000)
        };

        private readonly List<int> pipelines = new List<int>
        {
            3198184,
            3566374,
            3558964,
            3558991,
            3558922
        };
        #endregion

        private readonly Dictionary<string, CellFormat> columns = new Dictionary<string, CellFormat>()
        {
            { "A", new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "B",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "C",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "D",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "E",  new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00 руб" } } },
            { "F",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "PERCENT", Pattern = "# ### ###.00 %" } } },
            { "G",  new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00 руб" } } },
            { "H",  new CellFormat(){ NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###.00 дней" } } },
            { "I",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ### сек" } } }
        };

        #region Supplementary methods
        private void AddHeader(int? sheetId)
        {
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
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = ""} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Новых сделок"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Закрытых"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Успешно"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "На сумму"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Конверсия"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Средний чек"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Цикл сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Время ответа, сек"} }
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 168, 120, 84, 72, 108, 96, 120, 108, 144};
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
        }

        private async void PrepareSheets()
        {
            #region Retrieving spreadsheet
            requestContainer = new List<Request>();
            var spreadsheet = _service.Spreadsheets.Get(SpreadsheetId).Execute();
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
                            ColumnCount = columns.Count,
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
                            RowCount = 50,
                            ColumnCount = columns.Count,
                            FrozenRowCount = 1
                        },
                        Title = "Сводные",
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

            AddHeader(0);
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
                                ColumnCount = columns.Count,
                                FrozenRowCount = 1
                            },
                            Title = m.Item2,
                            SheetId = m.Item1
                        }
                    }
                });
                #endregion

                AddHeader(m.Item1);
            }

            #region Delete temp sheet
            requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = 31337 } });
            #endregion

            #region Executing request
            var batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = requestContainer;

            await _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).ExecuteAsync();
            #endregion
        }

        private void PrepareRow(int sheetId, string A, int B, int C, int D, int E, double H, double I)
        {
            #region Prepare data
            var rows = new List<RowData>();

            rows.Add(new RowData()
            {
                Values = new List<CellData>(){
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = A},
                             UserEnteredFormat = columns["A"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = B},
                             UserEnteredFormat = columns["B"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = C},
                             UserEnteredFormat = columns["C"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = D},
                             UserEnteredFormat = columns["D"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = E},
                             UserEnteredFormat = columns["E"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=INDIRECT(""R[0]C[-2]"", FALSE)/INDIRECT(""R[0]C[-3]"", FALSE)"},
                             UserEnteredFormat = columns["F"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=INDIRECT(""R[0]C[-2]"", FALSE)/INDIRECT(""R[0]C[-3]"", FALSE)"},
                             UserEnteredFormat = columns["G"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = H},
                             UserEnteredFormat = columns["H"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = I},
                             UserEnteredFormat = columns["I"] }
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

        private int GetLeadResponseTime(Lead lead)
        {
            List<int> replyTimestamps = new List<int>();

            int timeOfReference = (int)lead.created_at;

            #region Результат звонка
            if (lead.custom_fields_values != null)
            {
                var cf = lead.custom_fields_values.FirstOrDefault(x => x.field_id == 644675);
                if (cf != null)
                {
                    var cfValue = (string)cf.values[0].value;
                    if (cfValue == "Принят" || cfValue == "Ручная сделка") return 0;
                }
            }
            #endregion

            var leadEvents = leadRepo.GetEvents(lead.id);

            #region Смена ответственного
            if ((leadEvents != null) &&
                leadEvents.Where((x => x.type == "entity_responsible_changed")).Any(x => x.value_before[0].responsible_user.id == 2576764))
                timeOfReference = (int)leadEvents.Where((x => x.type == "entity_responsible_changed")).First(x => x.value_before[0].responsible_user.id == 2576764).created_at;
            #endregion

            #region Исходящие сообщения в чат
            if (leadEvents != null)
                foreach (var e in leadEvents)
                    if (e.type == "outgoing_chat_message")
                        replyTimestamps.Add((int)e.created_at);
            #endregion

            #region Исходящее письмо
            var notes = leadRepo.GetNotes(lead.id);
            if (notes != null)
                foreach (var n in notes)
                    if ((n.note_type == "amomail_message") && (n.parameters.income == false))
                        replyTimestamps.Add((int)n.created_at);
            #endregion

            #region Звонки
            if (lead._embedded.contacts != null)
                foreach (var c in lead._embedded.contacts)
                {
                    var contactEvents = contRepo.GetEvents(c.id);
                    if (contactEvents != null)
                        foreach (var e in contactEvents)
                        {
                            if ((e.type == "outgoing_call") || (e.type == "incoming_call"))
                            {
                                var callNote = contRepo.GetNoteById(e.value_after[0].note.id);
                                int duration = 0;

                                if (callNote.parameters != null && callNote.parameters.duration > 0)
                                    duration = (int)callNote.parameters.duration;

                                int actualCallTime = (int)e.created_at - duration;

                                if ((e.type == "outgoing_call") && (actualCallTime > lead.created_at))
                                    replyTimestamps.Add(actualCallTime);
                                else if ((duration > 0) && (actualCallTime > lead.created_at))
                                {
                                    replyTimestamps.Add(actualCallTime);
                                }
                            }
                        }
                }
            #endregion

            replyTimestamps.Add(timeOfReference + 86400);
            int result = replyTimestamps.Select(x => x - timeOfReference).Min();

            return result;
        }
        
        private double GetAverageResponseTime(IEnumerable<Lead> leads)
        {
            List<int> responseTimes = new List<int>();
            List<(int, int)> check = new List<(int, int)>();
            foreach (var lead in leads)
            { var rTime = GetLeadResponseTime(lead);
                responseTimes.Add(rTime); 
                check.Add((lead.id, rTime));
            }
            
            return responseTimes.Where(x => (x > 0) && (x < 86400)).Average();
        }

        private async void ProcessManager((int, string) m, (int,int) d)
        {
            requestContainer = new List<Request>();

            #region Список новых сделок в воронках из pipelines
            List<Lead> newLeads = new List<Lead>();
            foreach (var p in pipelines)
            {
                var leadsOpened = leadRepo.GetByCriteria($"filter[pipeline_id][0]={p}&filter[created_at][from]={d.Item1}&filter[created_at][to]={d.Item2}&filter[responsible_user_id]={m.Item1}&with=contacts");
                if (leadsOpened != null)
                    newLeads.AddRange(leadsOpened);
            }
            int totalNewLeads = newLeads.Count;
            double responseTime = GetAverageResponseTime(newLeads);
            #endregion

            #region Список закрытых сделок
            List<Lead> allLeads = new List<Lead>();
            var leadsClosed = leadRepo.GetByCriteria($"filter[pipeline_id][0]=3198184&filter[closed_at][from]={d.Item1}&filter[closed_at][to]={d.Item2}&filter[responsible_user_id]={m.Item1}");
            if (leadsClosed != null) allLeads.AddRange(leadsClosed);
            #endregion

            #region Даты
            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(d.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(d.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";
            #endregion

            #region Количество закрытых сделок
            int finishedLeads = allLeads.Where(x => (x.status_id == 142) || (x.status_id == 143)).Count();
            #endregion

            #region Количество успешных сделок
            int successLeads = allLeads.Where(x => x.status_id == 142).Count();
            #endregion

            #region Всего продаж
            int totalSales = allLeads.Where(x => x.status_id == 142).Sum(n => (int)n.price);
            #endregion

            #region Время сделки
            double averageTime = 0;
            if (finishedLeads > 0)
                averageTime = allLeads
                    .Where(x => (x.status_id == 142) || (x.status_id == 143))
                    .Select(x => (int)x.closed_at - (int)x.created_at).Average() / 86400;
            #endregion

            PrepareRow(m.Item1, dates, totalNewLeads, finishedLeads, successLeads, totalSales, averageTime, responseTime);

            #region Updating sheet
            if (requestContainer.Any())
            {
                var batchRequest = new BatchUpdateSpreadsheetRequest();
                batchRequest.Requests = requestContainer;

                await _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).ExecuteAsync();
            }
            #endregion

            GC.Collect();
        }

        private async void FinalizeManagers()
        {
            requestContainer = new List<Request>();

            foreach (var m in managers)
            {
                #region Prepare data
                var rows = new List<RowData>();

                rows.Add(new RowData()
                {
                    Values = new List<CellData>(){
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = "Среднее:"},
                             UserEnteredFormat = columns["A"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(B2:B{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["B"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(C2:C{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["C"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(D2:D{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["D"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(E2:E{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["E"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(F2:F{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["F"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(G2:G{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["G"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(H2:H{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["H"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(I2:I{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["I"] }
                }
                });
                #endregion

                #region Add request
                requestContainer.Add(new Request()
                {
                    UpdateCells = new UpdateCellsRequest()
                    {
                        Fields = '*',
                        Rows = rows,
                        Range = new GridRange()
                        {
                            SheetId = m.Item1,
                            StartRowIndex = dataRanges.Count + 1,
                            EndRowIndex = dataRanges.Count + 2,
                            StartColumnIndex = 0,
                            EndColumnIndex = columns.Count()
                        }
                    }
                });
                #endregion

                #region Add banding
                requestContainer.Add(new Request()
                {
                    AddBanding = new AddBandingRequest()
                    {
                        BandedRange = new BandedRange()
                        {
                            Range = new GridRange() { SheetId = m.Item1, StartRowIndex = 1, EndRowIndex = dataRanges.Count + 1 },
                            RowProperties = new BandingProperties() 
                            { 
                                FirstBandColor = new Color() { Red = 217f / 255, Green = 234f / 255, Blue = 211f / 255 },
                                SecondBandColor = new Color() { Red = 182f / 255, Green = 215f / 255, Blue = 168f / 255 },
                            }
                        }
                    }
                });
                #endregion
            }

            #region Update sheet
            if (requestContainer.Any())
            {
                var batchRequest = new BatchUpdateSpreadsheetRequest();
                batchRequest.Requests = requestContainer;

                await _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).ExecuteAsync();
            }
            #endregion
        }

        private async void FinalizeTotals()
        {
            requestContainer = new List<Request>();

            foreach (var m in managers)
            {
                #region Prepare data
                var rows = new List<RowData>();

                rows.Add(new RowData()
                {
                    Values = new List<CellData>(){
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = $"{m.Item2}"},
                             UserEnteredFormat = columns["A"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!B{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["B"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!C{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["C"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!D{dataRanges.Count + 2}"},
                             UserEnteredFormat = columns["D"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!E{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["E"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!F{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["F"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!G{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["G"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!H{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["H"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!I{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["I"] }
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
                        SheetId = 0
                    }
                });
                #endregion            
            }

            #region Add banding
            requestContainer.Add(new Request()
            {
                AddBanding = new AddBandingRequest()
                {
                    BandedRange = new BandedRange()
                    {
                        Range = new GridRange() { SheetId = 0, StartRowIndex = 1, EndRowIndex = managers.Count + 1 },
                        BandedRangeId = 0,
                        RowProperties = new BandingProperties()
                        {
                            FirstBandColor = new Color() { Red = 217f / 255, Green = 234f / 255, Blue = 211f / 255 },
                            SecondBandColor = new Color() { Red = 182f / 255, Green = 215f / 255, Blue = 168f / 255 },
                        }
                    }
                }
            });
            #endregion

            #region Update sheet
            if (requestContainer.Any())
            {
                var batchRequest = new BatchUpdateSpreadsheetRequest();
                batchRequest.Requests = requestContainer;

                await _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).ExecuteAsync();
            }
            #endregion
        }
        #endregion

        #region Realization
        public void Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove("report_data");
                return;
            }

            PrepareSheets();

            foreach (var d in dataRanges)
                foreach (var m in managers)
                {
                    if (_token.IsCancellationRequested) break;

                    ProcessManager(m, d);
                }

            FinalizeManagers();
            FinalizeTotals();

            _processQueue.Remove("report_data");
        }
        #endregion
    }
}