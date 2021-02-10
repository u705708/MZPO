﻿using Google.Apis.Sheets.v4;
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
    public class RetailKPIProcessor : IProcessor
    {
        #region Definition
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly SheetsService _service;
        private readonly string SpreadsheetId;
        private readonly IAmoRepo<Lead> leadRepo;
        private readonly IAmoRepo<Contact> contRepo;
        private readonly long endDate;
        protected readonly CancellationToken _token;

        public RetailKPIProcessor(AmoAccount acc, GSheets gSheets, string spreadsheetId, TaskList processQueue, long dateTo, CancellationToken token)
        {
            _acc = acc;
            _processQueue = processQueue;
            _token = token;
            _service = gSheets.GetService();
            SpreadsheetId = spreadsheetId;
            leadRepo = _acc.GetRepo<Lead>();
            contRepo = _acc.GetRepo<Contact>();
            endDate = dateTo;
        }

        private List<(int?, int, int)> longAnsweredLeads;
        private IEnumerable<Event> inCalls;
        private IEnumerable<Event> outCalls;

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
            (6410290, "Вероника Бармина"),
            (6699043, "Татьяна Ганоу"),
            (6729241, "Серик Айбасов")
        };

        private readonly List<(int, int)> dataRanges = new List<(int, int)>
        {
            (1601499600,1604177999),    //октябрь
            (1604178000,1606769999),    //ноябрь
            (1606770000,1609448399),    //декабрь
            (1609448400,1612126799),    //январь
            //(1612126800,1614545999)     //февраль
        };

        private readonly List<int> pipelines = new List<int>
        {
            3198184,
            3566374,
            3558964,
            3558991,
            3558922
        };

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
            { "I",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ### сек" } } },
            { "J",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "K",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "L",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "M",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } }
        };
        #endregion

        #region Supplementary methods
        private List<Request> GetHeaderRequests(int? sheetId)
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
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = ""} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Новых сделок"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Закрытых"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Успешно"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "На сумму"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Конверсия"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Средний чек"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Цикл сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Время ответа, сек"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Долгие сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Входящие"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Исходящие"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Пропущенные"} }
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 168, 120, 84, 72, 108, 96, 120, 108, 144, 120, 108, 108, 108};
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

        private Request GetRowRequest(int sheetId, string A, int B, int C, int D, int E, double H, double I, int J, int K, int L, int M)
        {
            #region Prepare data
            var rows = new List<RowData>
            {
                new RowData()
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
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=IF(INDIRECT(""R[0]C[-3]"", FALSE) = 0, 0, INDIRECT(""R[0]C[-2]"", FALSE)/INDIRECT(""R[0]C[-3]"", FALSE))"},
                             UserEnteredFormat = columns["F"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=IF(INDIRECT(""R[0]C[-3]"", FALSE) = 0, 0, INDIRECT(""R[0]C[-2]"", FALSE)/INDIRECT(""R[0]C[-3]"", FALSE))"},
                             UserEnteredFormat = columns["G"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = H},
                             UserEnteredFormat = columns["H"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = I},
                             UserEnteredFormat = columns["I"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = J},
                             UserEnteredFormat = columns["J"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = K},
                             UserEnteredFormat = columns["K"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = L},
                             UserEnteredFormat = columns["L"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = M},
                             UserEnteredFormat = columns["M"] },
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

        private async Task PrepareSheets()
        {
            List<Request> requestContainer = new();

            #region Retrieving spreadsheet
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

            requestContainer.AddRange(GetHeaderRequests(0));
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

                requestContainer.AddRange(GetHeaderRequests(m.Item1));
            }

            #region Delete temp sheet
            requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = 31337 } });
            #endregion

            await UpdateSheetsAsync(requestContainer);
        }
        
        private void CalculateDataRanges()
        {
            return;
        }

        private async Task PopulateCalls((int, int) dataRange)
        {
            #region Даты
            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(dataRange.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(dataRange.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";
            #endregion

            _processQueue.UpdateTaskName("report_kpi", $"KPIReport: {dates}, collecting calls");
            List<Task> tasks = new();

            tasks.Add(Task.Run(() => outCalls = contRepo.GetEventsByCriteria($"filter[type]=outgoing_call&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}")));
            tasks.Add(Task.Run(() => inCalls = contRepo.GetEventsByCriteria($"filter[type]=incoming_call&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}")));

            await Task.WhenAll(tasks);

            _processQueue.UpdateTaskName("report_kpi", $"KPIReport: {dates}, processing managers");
        }

        private async Task FinalizeManagers()
        {
            List<Request> requestContainer = new();

            foreach (var m in managers)
            {
                #region Prepare data
                var rows = new List<RowData>
                {
                    new RowData()
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
                             UserEnteredFormat = columns["I"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(J2:J{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["J"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(K2:K{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["K"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(L2:L{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["L"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"=AVERAGE(M2:M{dataRanges.Count + 1})" },
                             UserEnteredFormat = columns["M"] },
                }
                    }
                };
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
                            EndColumnIndex = columns.Count
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

            await UpdateSheetsAsync(requestContainer);
        }

        private async Task FinalizeTotals()
        {
            List<Request> requestContainer = new();

            foreach (var m in managers)
            {
                #region Prepare data
                var rows = new List<RowData>
                {
                    new RowData()
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
                             UserEnteredFormat = columns["I"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!J{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["J"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!K{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["K"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!L{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["L"] },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ FormulaValue = $"='{m.Item2}'!M{dataRanges.Count + 2}" },
                             UserEnteredFormat = columns["M"] },
                }
                    }
                };
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

            await UpdateSheetsAsync(requestContainer);
        }

        private int GetLeadResponseTime(Lead lead)
        {
            List<int> replyTimestamps = new List<int>();

            int timeOfReference = (int)lead.created_at;

            #region Результат звонка
            if (lead.custom_fields_values is not null)
            {
                var cf = lead.custom_fields_values.FirstOrDefault(x => x.field_id == 644675);
                if (cf is not null)
                {
                    var cfValue = (string)cf.values[0].value;
                    if (cfValue == "Принят" || cfValue == "Ручная сделка") return 0;
                }
            }
            #endregion

            #region Время суток
            var dt = DateTimeOffset.FromUnixTimeSeconds(timeOfReference).UtcDateTime;
            if (dt.Hour > 17)
                timeOfReference = (int)((DateTimeOffset)new DateTime(dt.Year, dt.Month, dt.Day, 11, 0, 0).AddDays(1)).ToUnixTimeSeconds();
            else if (dt.Hour < 6)
                timeOfReference = (int)((DateTimeOffset)new DateTime(dt.Year, dt.Month, dt.Day, 11, 0, 0)).ToUnixTimeSeconds();
            #endregion

            var allEvents = new List<Event>();
            var allNotes = new List<Note>();

            allEvents.AddRange(leadRepo.GetEntityEvents(lead.id));
            allNotes.AddRange(leadRepo.GetEntityNotes(lead.id));

            #region Смена ответственного
            if (allEvents
                    .Where(x => x.type == "entity_responsible_changed")
                    .Any(x => x.value_before[0].responsible_user.id == 2576764 &&                                   //Если меняли ответственного с Администартора на текущего
                        x.value_after[0].responsible_user.id == lead.responsible_user_id))
                timeOfReference = (int)allEvents
                    .Where((x => x.type == "entity_responsible_changed"))
                    .First(x => x.value_before[0].responsible_user.id == 2576764)
                    .created_at;
            else if (allEvents
                    .Where(x => x.type == "entity_responsible_changed")                                             //Если меняли на текущего
                    .Any(x => x.value_after[0].responsible_user.id == lead.responsible_user_id) &&
                    allEvents
                    .Where(x => x.type == "entity_responsible_changed")                                             //И с Администратора
                    .Any(x => x.value_before[0].responsible_user.id == 2576764))
                timeOfReference = (int)allEvents
                    .Where((x => x.type == "entity_responsible_changed"))
                    .First(x => x.value_before[0].responsible_user.id == 2576764)
                    .created_at;
            #endregion

            #region Собираем данные из контактов
            if (lead._embedded.contacts is not null)
                Parallel.ForEach(lead._embedded.contacts, contact =>
                {
                    var events = contRepo.GetEntityEvents(contact.id);
                    lock (allEvents)
                    {
                        allEvents.AddRange(events);
                    }
                    var notes = contRepo.GetEntityNotes(contact.id);
                    lock (allNotes)
                    {
                        allNotes.AddRange(notes);
                    }
                });
            #endregion

            #region Cообщения в чат
            foreach (var e in allEvents)
                if ((e.type == "outgoing_chat_message") || (e.type == "incoming_chat_message"))
                    replyTimestamps.Add((int)e.created_at);
            #endregion

            #region Исходящее письмо
            foreach (var n in allNotes)
                if ((n.note_type == "amomail_message") && (n.parameters.income == false))
                    replyTimestamps.Add((int)n.created_at);
            #endregion

            #region Звонки
            foreach (var e in allEvents)
            {
                if ((e.type == "outgoing_call") || (e.type == "incoming_call"))
                {
                    Note callNote;

                    if (allNotes.Any(x => x.id == e.value_after[0].note.id))
                        callNote = allNotes.First(x => x.id == e.value_after[0].note.id);
                    else callNote = contRepo.GetNoteById(e.value_after[0].note.id);

                    int duration = 0;

                    if (callNote.parameters is not null && callNote.parameters.duration > 0)
                        duration = (int)callNote.parameters.duration;

                    int actualCallTime = (int)e.created_at - duration;

                    if ((e.type == "outgoing_call") && (actualCallTime > lead.created_at))
                        replyTimestamps.Add(actualCallTime);
                    else if ((duration > 0) && (actualCallTime > lead.created_at))
                        replyTimestamps.Add(actualCallTime);
                }
            }
            #endregion

            replyTimestamps.Add(timeOfReference + 86400);

            return replyTimestamps.AsParallel().Select(x => x - timeOfReference).Where(x => x > -600).Min();
        }

        private double GetAverageResponseTime(IEnumerable<Lead> leads)
        {
            List<int> responseTimes = new List<int>();
            
            Parallel.ForEach(leads, x => {
                var rTime = GetLeadResponseTime(x);
                responseTimes.Add(rTime);

                if (rTime > 3600)
                    lock (longAnsweredLeads)
                    {
                        longAnsweredLeads.Add((x.responsible_user_id, x.id, rTime));
                    }
            });

            if (responseTimes.AsParallel().Any(x => (x > 0) && (x < 3600)))
                return responseTimes.AsParallel().Where(x => (x > 0) && (x < 3600)).Average();
            else return 0;
        }

        private async Task ProcessManager((int, string) manager, (int, int) dataRange)
        {
            #region Даты
            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(dataRange.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(dataRange.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";
            #endregion

            #region Список новых сделок в воронках из pipelines
            _processQueue.AddSubTask("report_kpi", $"report_kpi_{manager.Item2}", $"KPIReport: {dates}, new leads");

            List<Lead> newLeads = new List<Lead>();

            Parallel.ForEach(pipelines, p => {
                var range = leadRepo.GetByCriteria($"filter[pipeline_id][0]={p}&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&filter[responsible_user_id]={manager.Item1}&with=contacts");
                lock (newLeads)
                {
                    newLeads.AddRange(range);
                }
            });

            int totalNewLeads = newLeads.Count;

            _processQueue.UpdateTaskName($"report_kpi_{manager.Item2}", $"KPIReport: {dates}, new leads: {totalNewLeads}");

            double responseTime = GetAverageResponseTime(newLeads);
            int longLeads = longAnsweredLeads.Count(x => x.Item1 == manager.Item1);
            #endregion

            #region Список закрытых сделок
            _processQueue.UpdateTaskName($"report_kpi_{manager.Item2}", $"KPIReport: {dates}, closed leads");

            var allLeads = leadRepo.GetByCriteria($"filter[pipeline_id][0]=3198184&filter[closed_at][from]={dataRange.Item1}&filter[closed_at][to]={dataRange.Item2}&filter[responsible_user_id]={manager.Item1}");
            #endregion

            #region Количество закрытых сделок
            int finishedLeads = allLeads.Where(x => (x.status_id == 142) || (x.status_id == 143)).Count();
            #endregion

            #region Количество успешных сделок
            int successLeads = allLeads.Where(x => x.status_id == 142).Count();
            #endregion

            #region Количество исходящих вызовов
            int outCallsCount = outCalls.Count(x => x.created_by == manager.Item1);
            #endregion

            #region Количество входящих вызовов
            int inCallsCount = inCalls.Count(x => x.created_by == manager.Item1);
            #endregion

            #region Количество пропущенных вызовов
            _processQueue.UpdateTaskName($"report_kpi_{manager.Item2}", $"KPIReport: {dates}, missed calls");

            int missedCallsCount = 0;

            var callIdList = new List<int>();

            foreach (var e in inCalls.Where(x => x.created_by == manager.Item1))
                callIdList.Add(e.value_after[0].note.id);

            foreach (var n in contRepo.BulkGetNotesById(callIdList))
            {
                int duration = -1;

                if (n.parameters is not null)
                    duration = (int)n.parameters.duration;

                if (duration == 0) missedCallsCount++;
            }
            #endregion

            #region Всего продаж
            int totalSales = allLeads.Where(x => x.status_id == 142).Sum(n => (int)n.price);
            #endregion

            #region Время сделки
            double averageTime = 0;
            if (finishedLeads > 0)
                averageTime = allLeads.AsParallel()
                    .Where(x => (x.status_id == 142) || (x.status_id == 143))
                    .Select(x => (int)x.closed_at - (int)x.created_at).Average() / 86400;
            #endregion

            List<Request> requestContainer = new()
            {
                GetRowRequest(manager.Item1, dates, totalNewLeads, finishedLeads, successLeads, totalSales, averageTime, responseTime, longLeads, inCallsCount, outCallsCount, missedCallsCount)
            };

            await UpdateSheetsAsync(requestContainer);

            _processQueue.Remove($"report_kpi_{manager.Item2}");
        }

        private async Task UpdateSheetsAsync(List<Request> requestContainer)
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
        public async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove("report_kpi");
                return;
            }

            Log.Add("Started KPI report.");

            CalculateDataRanges();

            await PrepareSheets();

            foreach (var d in dataRanges)
            {
                if (_token.IsCancellationRequested) break;
                longAnsweredLeads = new();
                List<Task> tasks = new();

                await PopulateCalls(d);

                foreach (var manager in managers)
                {
                    if (_token.IsCancellationRequested) break;
                    var m = manager;
                    tasks.Add(Task.Run(() => ProcessManager(m, d), _token));
                }
                
                await Task.WhenAll(tasks);
            }

            await FinalizeManagers();
            await FinalizeTotals();

            Log.Add("Finished KPI report.");

            _processQueue.Remove("report_kpi");
        }
        #endregion
    }
}