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
    internal class LongLeadsProcessor : AbstractReportProcessor, IReportProcessor
    {
        #region Definition
        /// <summary>
        /// Формирует отчёт для отдела розницы. Проверяет в сделках время ответа клиента. Выводит список сделок, где время ответа больше часа.
        /// </summary>
        internal LongLeadsProcessor(AmoAccount acc, TaskList processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
            dataRanges = new() { ((int)dateFrom, (int)dateTo) };
        }

        private readonly List<(int, int)> dataRanges;

        private readonly List<(int, string)> managers = new()
        {
            (2375107, "Кристина Гребенникова"),
            (2375143, "Екатерина Белоусова"),
            (2976226, "Вера Гладкова"),
            (3835801, "Наталья Кубышина"),
            (6158035, "Анастасия Матюк"),
            (6769426, "Рюмина Наталья"),
            (6872548, "Оксана Полукеева"),
            (2375152, "Карен Оганисян"),
            (3813670, "Федорова Александра"),
            (6102562, "Валерия Лукьянова"),
            (6410290, "Вероника Бармина"),
            (6729241, "Серик Айбасов"),
            (6890059, "Аскер Абулгазинов"),
        };

        private readonly List<int> pipelines = new()
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

        private readonly Dictionary<string, CellFormat> columnsFormat = new()
        {
            { "A", new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "B",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "C",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "D",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ### сек" } } },
            { "E",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "F",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "PERCENT", Pattern = "# ### ###.00 %" } } },
            { "G",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "H",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "PERCENT", Pattern = "# ### ###.00 %" } } },
            { "I",  new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "TEXT" } } },
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
                    Bold = false,
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
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Долгие сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Время ответа, сек"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Время ответа"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Процент долгих"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Всего нереализовано"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Процент нереализованных"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = ""} },
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 148, 134, 108, 148, 128, 116, 154, 192, 166 };
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

        private CellData[] GetCellData(string A, int B, double D)
        {
            return new []{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = A},
                    UserEnteredFormat = columnsFormat["A"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = B},
                    UserEnteredFormat = columnsFormat["B"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=COUNTA(INDIRECT(""R[3]C[-2]"", FALSE):INDIRECT(""R[999]C[-2]"", FALSE))"},
                    UserEnteredFormat = columnsFormat["C"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = D},
                    UserEnteredFormat = columnsFormat["D"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=TEXTJOIN("" "", TRUE, TO_TEXT(QUOTIENT(INDIRECT(""R[0]C[-1]"", FALSE),60)), ""мин"", TO_TEXT(MOD(INDIRECT(""R[0]C[-1]"", FALSE),60)))"},
                    UserEnteredFormat = columnsFormat["E"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=INDIRECT(""R[0]C[-3]"", FALSE)/INDIRECT(""R[0]C[-4]"", FALSE)"},
                    UserEnteredFormat = columnsFormat["F"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=COUNTIF(INDIRECT(""R[3]C[2]"", FALSE):INDIRECT(""R[999]C[2]"", FALSE), ""Закрыто и не реализовано"")"},
                    UserEnteredFormat = columnsFormat["G"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=INDIRECT(""R[0]C[-1]"", FALSE)/(INDIRECT(""R[0]C[-1]"", FALSE)+COUNTIF(INDIRECT(""R[3]C[1]"", FALSE):INDIRECT(""R[999]C[1]"", FALSE), ""Успешно реализовано""))"},
                    UserEnteredFormat = columnsFormat["H"] },
            };
        }

        private string GetStatus(int id)
        {
            if (!statuses.ContainsKey(id)) return id.ToString();
            return statuses[id];
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

        private void ProcessManager((int, string) manager, (int, int) dataRange)
        {
            //Даты
            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(dataRange.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(dataRange.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";

            //Список новых сделок в воронках из pipelines
            _processQueue.AddSubTask(_taskName, $"{_taskName}_{manager.Item2}", $"WeeklyReport: {dates}, new leads");

            List<Lead> newLeads = new();

            Parallel.ForEach(
                pipelines,
                new ParallelOptions { MaxDegreeOfParallelism = 3 },
                p => {
                var range = _leadRepo.GetByCriteria($"filter[pipeline_id][0]={p}&filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&filter[responsible_user_id]={manager.Item1}&with=contacts");
                lock (newLeads)
                {
                    newLeads.AddRange(range);
                }
            });

            int totalNewLeads = newLeads.Count;

            _processQueue.UpdateTaskName($"{_taskName}_{manager.Item2}", $"WeeklyReport: {dates}, new leads: {totalNewLeads}");

            double responseTime = GetAverageResponseTime(newLeads, _longAnsweredLeads, _leadRepo, _contRepo);
            int longLeads = _longAnsweredLeads.Count(x => x.Item1 == manager.Item1);

            List<Request> requestContainer = new();

            requestContainer.Add(GetRowRequest(manager.Item1, GetCellData(dates, totalNewLeads, responseTime)));

            UpdateSheetsAsync(requestContainer, _service, _spreadsheetId).Wait();

            _processQueue.Remove($"{_taskName}_{manager.Item2}");
        }

        private async Task FinalizeManagers()
        {
            List<Request> requestContainer = new();

            foreach (var m in managers)
            {
                #region Prepare Data
                List<(int?, int, int, int?)> leads = new();
                if (_longAnsweredLeads.Any(x => x.Item1 == m.Item1))
                    leads.AddRange(_longAnsweredLeads.Where(x => x.Item1 == m.Item1));
                var rows = new List<RowData>();

                #region Header
                rows.Add(new RowData()
                {
                    Values = new List<CellData>(){
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Сделка" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Время ответа, сек" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Время ответа" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Примечания" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
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
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
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
                            EndColumnIndex = 9
                        }
                    }
                });
                #endregion

                #region Merge cells
                for (int i = 0; i <= leads.Count; i++)
                {
                    requestContainer.Add(new Request()
                    {
                        MergeCells = new MergeCellsRequest()
                        {
                            Range = new GridRange()
                            {
                                SheetId = m.Item1,
                                StartRowIndex = dataRanges.Count + i + 2,
                                EndRowIndex = dataRanges.Count + i + 3,
                                StartColumnIndex = 3,
                                EndColumnIndex = 8
                            }
                        }

                    });
                }
                #endregion
            }

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }
        #endregion

        #region Realization
        public override async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove(_taskName);
                return;
            }

            await PrepareSheets();

            foreach (var d in dataRanges)
            {
                if (_token.IsCancellationRequested) break;
                _longAnsweredLeads = new();
                List<Task> tasks = new();

                foreach (var manager in managers)
                {
                    if (_token.IsCancellationRequested) break;
                    var m = manager;
                    tasks.Add(Task.Run(() => ProcessManager(m, d), _token));
                }

                await Task.WhenAll(tasks);
            }

            await FinalizeManagers();

            _processQueue.Remove(_taskName);
        }
        #endregion
    }
}