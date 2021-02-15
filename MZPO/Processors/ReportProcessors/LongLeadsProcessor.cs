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
    public class LongLeadsProcessor : IProcessor
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

        public LongLeadsProcessor(AmoAccount acc, GSheets gSheets, string spreadsheetId, TaskList processQueue, long dateTo, CancellationToken token)
        {
            _acc = acc;
            _processQueue = processQueue;
            _token = token;
            _service = gSheets.GetService();
            SpreadsheetId = spreadsheetId;
            leadRepo = _acc.GetRepo<Lead>();
            contRepo = _acc.GetRepo<Contact>();

            dataRanges = new List<(int, int)>();
            endDate = dateTo;
        }

        private List<(int?, int, int, int?)> longAnsweredLeads;
        private IEnumerable<Event> inCalls;
        private IEnumerable<Event> outCalls;

        private readonly List<(int, int)> dataRanges;

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

        private readonly List<int> pipelines = new List<int>
        {
            3198184,
            3566374,
            3558964,
            3558991,
            3558922
        };

        private readonly Dictionary<int, string> statuses = new()
        {
            { 32532877, "Неразобранное" },
            { 32532880, "Получен новый лид" },
            { 36592555, "Не дозвонились" },
            { 32532883, "Взято в работу" },
            { 32532886, "Отправлено КП" },
            { 32533195, "Назначена встреча" },
            { 32533198, "Проведена встреча" },
            { 33625285, "документы получены" },
            { 33817816, "Выставлен счёт" },
            { 32533201, "Получена предоплата" },
            { 32533204, "Получена полная оплата" },
            { 142, "Успешно реализовано" },
            { 143, "Закрыто и не реализовано" },
            { 35051836, "6 месяцев (Отложенные)" },
            { 35051839, "5 месяцев (Отложенные)" },
            { 35051842, "4 месяца (Отложенные)" },
            { 35051815, "3 месяца (Отложенные)" },
            { 35051845, "2 месяца (Отложенные)" },
            { 35051818, "1 месяц (Отложенные)" },
            { 35301991, "3 недели (Отложенные)" },
            { 35051821, "2 недели (Отложенные)" },
            { 35301931, "1 неделя (Отложенные)" },
            { 35002570, "Новая сделка (Вызревание)" },
            { 35002573, "1 месяц (Вызревание)" },
            { 35002576, "2 месяца (Вызревание)" },
            { 35002579, "3 месяца (Вызревание)" },
            { 35002582, "4 месяца (Вызревание)" },
            { 35002585, "5 месяцев (Вызревание)" },
            { 35002588, "6 месяцев (Вызревание)" },
            { 35002591, "8 месяцев (Вызревание)" },
            { 35002594, "10 месяцев (Вызревание)" },
            { 35002597, "12 месяцев (Вызревание)" },
            { 33496813, "Первичный контакт (Обучение)" },
            { 31100218, "НОВЫЙ ЛИД (Корпоративный отдел)" },
            { 31100221, "Переговоры (Корпоративный отдел)" },
            { 31100224, "Принимают решение (Корпоративный отдел)" },
            { 35942104, "Перенос в Корпоративный Отдел" },
            { 35075311, "Чемпионат" },
            { 35075338, "ДОД" },
            { 32866090, "Модели" },
            { 32544562, "Вебинары" },
            { 34278559, "подписка на рассылку банки/IT/бухгалтерия" },
            { 31653277, "ПОДПИСКА НА РАССЫЛКУ МЕДИЦИНА" },
            { 34278556, "подписка на рассылку парикмахеры/визажисты/маникюристы" },
            { 35093539, "Партнеры" },
            { 35093542, "СПАМ" }
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
            var width = new List<int>() { 168, 120, 84, 72, 108, 96, 120, 108, 144, 120, 108, 108, 108 };
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

        private void CalculateDataRanges()
        {
            DateTime dt = DateTimeOffset.FromUnixTimeSeconds(endDate).UtcDateTime;

            var d2_2 = dt;
            var d2_1 = new DateTime(d2_2.Year, d2_2.Month, 1, 2, 0, 0);
            var d1_2 = d2_2.AddMonths(-1);
            var d1_1 = d2_1.AddMonths(-1);

            var dr2_2 = (int)((DateTimeOffset)d2_2).ToUnixTimeSeconds();
            var dr2_1 = (int)((DateTimeOffset)d2_1).ToUnixTimeSeconds();
            var dr1_2 = (int)((DateTimeOffset)d1_2).ToUnixTimeSeconds();
            var dr1_1 = (int)((DateTimeOffset)d1_1).ToUnixTimeSeconds();

            //dataRanges.Add((dr1_1, dr1_2));
            //dataRanges.Add((dr2_1, dr2_2));

            dataRanges.Add((1610294400, 1612886399));
            //dataRanges.Add((1612294400, 1612886399));
        }

        private async Task PrepareSheets()
        {
            List<Request> requestContainer = new();

            #region Retrieving spreadsheet
            var spreadsheet = _service.Spreadsheets.Get(SpreadsheetId).Execute();
            #endregion

            #region Deleting existing sheets except first
            foreach (var s in spreadsheet.Sheets)
            {
                if (s.Properties.SheetId == 0) continue;
                requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = s.Properties.SheetId } });
            }
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

            await UpdateSheetsAsync(requestContainer);
        }

        private async Task PopulateCalls((int, int) dataRange)
        {
            #region Даты
            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(dataRange.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(dataRange.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";
            #endregion

            _processQueue.UpdateTaskName("report_retail", $"WeeklyReport: {dates}, collecting calls");
            List<Task> tasks = new();

            tasks.Add(Task.Run(() => outCalls = contRepo.GetEventsByCriteria($"filter[type]=outgoing_call&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}")));
            tasks.Add(Task.Run(() => inCalls = contRepo.GetEventsByCriteria($"filter[type]=incoming_call&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}")));

            await Task.WhenAll(tasks);

            _processQueue.UpdateTaskName("report_retail", $"WeeklyReport: {dates}, processing managers");
        }

        private async Task FinalizeManagers()
        {
            List<Request> requestContainer = new();

            foreach (var m in managers)
            {
                #region Prepare Data
                List<(int?, int, int, int?)> leads = new();
                if (longAnsweredLeads.Any(x => x.Item1 == m.Item1))
                    leads.AddRange(longAnsweredLeads.Where(x => x.Item1 == m.Item1));
                var rows = new List<RowData>();

                #region Header
                rows.Add(new RowData()
                {
                    Values = new List<CellData>(){
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Сделка" } },
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Время ответа, сек" } },
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Время ответа" } },
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Примечания" } },
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Статус сделки" } },
                    }
                });
                #endregion

                foreach (var l in leads)
                {
                    string status = GetStatus((int)l.Item4);
                    rows.Add(new RowData()
                    {
                        Values = new List<CellData>(){
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ FormulaValue = $@"=HYPERLINK(""https://mzpoeducationsale.amocrm.ru/leads/detail/{l.Item2}"", ""{l.Item2}"")" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = $"{l.Item3}" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ FormulaValue = $@"=TEXTJOIN("" "", TRUE, TO_TEXT(QUOTIENT(INDIRECT(""R[0]C[-1]"", FALSE),3600)), ""ч."", TO_TEXT(QUOTIENT(MOD(INDIRECT(""R[0]C[-1]"", FALSE),3600), 60)), ""мин."")" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = $"{status}" } }
                        }
                    });
                }
                #endregion

                #region Add Request
                requestContainer.Add(new Request()
                {
                    UpdateCells = new UpdateCellsRequest()
                    {
                        Fields = '*',
                        Rows = rows,
                        Range = new GridRange()
                        {
                            SheetId = m.Item1,
                            StartRowIndex = dataRanges.Count + 2,
                            EndRowIndex = dataRanges.Count + 2 + rows.Count,
                            StartColumnIndex = 0,
                            EndColumnIndex = 5
                        }
                    }
                });
                #endregion
            }

            await UpdateSheetsAsync(requestContainer);
        }

        private string GetStatus(int id)
        {
            if (!statuses.ContainsKey(id)) return id.ToString();
            return statuses[id];
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
                        longAnsweredLeads.Add((x.responsible_user_id, x.id, rTime, x.status_id));
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
            _processQueue.AddSubTask("report_retail", $"report_retail_{manager.Item2}", $"WeeklyReport: {dates}, new leads");

            List<Lead> newLeads = new List<Lead>();

            Parallel.ForEach(pipelines, p => {
                var range = leadRepo.GetByCriteria($"filter[pipeline_id][0]={p}&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&filter[responsible_user_id]={manager.Item1}&with=contacts");
                lock (newLeads)
                {
                    newLeads.AddRange(range);
                }
            });

            int totalNewLeads = newLeads.Count;

            _processQueue.UpdateTaskName($"report_retail_{manager.Item2}", $"WeeklyReport: {dates}, new leads: {totalNewLeads}");

            double responseTime = GetAverageResponseTime(newLeads);
            int longLeads = longAnsweredLeads.Count(x => x.Item1 == manager.Item1);
            #endregion

            #region Список закрытых сделок
            _processQueue.UpdateTaskName($"report_retail_{manager.Item2}", $"WeeklyReport: {dates}, closed leads");

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
            _processQueue.UpdateTaskName($"report_retail_{manager.Item2}", $"WeeklyReport: {dates}, missed calls");

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

            List<Request> requestContainer = new();

            requestContainer.Add(GetRowRequest(manager.Item1, dates, totalNewLeads, finishedLeads, successLeads, totalSales, averageTime, responseTime, longLeads, inCallsCount, outCallsCount, missedCallsCount));

            await UpdateSheetsAsync(requestContainer);

            _processQueue.Remove($"report_retail_{manager.Item2}");
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
                _processQueue.Remove("report_retail");
                return;
            }

            Log.Add("Started weekly report.");

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

            Log.Add("Finished weekly report.");

            _processQueue.Remove("report_retail");
        }
        #endregion
    }
}