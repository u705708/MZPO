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
    internal class WeeklyKPIReportProcessor : AbstractReportProcessor, IReportProcessor
    {
        #region Definition
        private readonly string _KPISpreadsheetId;

        /// <summary>
        /// Формирует отчёт для отдела розницы, собирает показатели с начала месяца, сравнивает со среднемесячными показателями за аналогичный период.
        /// </summary>
        internal WeeklyKPIReportProcessor(AmoAccount acc, TaskList processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token) 
            :base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
            _KPISpreadsheetId = "1ZjdabzAtTQKKdK5ZtGfvYT2jA-JN6agO0QMxtWPed0k";

            dataRanges = new List<(int, int)>();
        }

        private double monthRatio;

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

        private readonly Dictionary<string, CellFormat> columnsFormat = new()
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

        private CellData[] GetCellData(string A, int B, int C, int D, int E, double H, double I, int J, int K, int L, int M)
        {
            return new[]{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = A},
                    UserEnteredFormat = columnsFormat["A"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = B},
                    UserEnteredFormat = columnsFormat["B"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = C},
                    UserEnteredFormat = columnsFormat["C"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = D},
                    UserEnteredFormat = columnsFormat["D"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = E},
                    UserEnteredFormat = columnsFormat["E"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=IF(INDIRECT(""R[0]C[-3]"", FALSE) = 0, 0, INDIRECT(""R[0]C[-2]"", FALSE)/INDIRECT(""R[0]C[-3]"", FALSE))"},
                    UserEnteredFormat = columnsFormat["F"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=IF(INDIRECT(""R[0]C[-3]"", FALSE) = 0, 0, INDIRECT(""R[0]C[-2]"", FALSE)/INDIRECT(""R[0]C[-3]"", FALSE))"},
                    UserEnteredFormat = columnsFormat["G"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = H},
                    UserEnteredFormat = columnsFormat["H"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = I},
                    UserEnteredFormat = columnsFormat["I"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = J},
                    UserEnteredFormat = columnsFormat["J"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = K},
                    UserEnteredFormat = columnsFormat["K"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = L},
                    UserEnteredFormat = columnsFormat["L"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = M},
                    UserEnteredFormat = columnsFormat["M"] },
            };
        }

        private void CalculateDateRange()
        {
            DateTime dt = DateTimeOffset.FromUnixTimeSeconds(_dateTo).UtcDateTime;

            var d2_2 = dt;
            var d2_1 = new DateTime(d2_2.Year, d2_2.Month, 1, 2, 0, 0);

            var dr2_2 = (int)((DateTimeOffset)d2_2).ToUnixTimeSeconds();
            var dr2_1 = (int)((DateTimeOffset)d2_1).ToUnixTimeSeconds();

            dataRanges.Add((dr2_1, dr2_2));

            monthRatio = ((dr2_2 - dr2_1) / 86400f) / 30.42;
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

        private async Task AddKPIData()
        {
            List<Request> requestContainer = new();

            var range = "Сводные!A:M";
            var request = _service.Spreadsheets.Values.Get(_KPISpreadsheetId, range);
            request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;
            var values = request.Execute().Values;

            if (values is not null)
                foreach (var row in values)
                {
                    if (managers.Any(x => x.Item2 == (string)row[0]))
                    {
                        string A = "Средние в % от мес";
                        var B = (int)(Convert.ToDouble(row[1]) * monthRatio);
                        var C = (int)(Convert.ToDouble(row[2]) * monthRatio);
                        var D = (int)(Convert.ToDouble(row[3]) * monthRatio);
                        var E = (int)(Convert.ToDouble(row[4]) * monthRatio);
                        var H = Convert.ToDouble(row[7]);
                        var I = Convert.ToDouble(row[8]);
                        var J = (int)(Convert.ToDouble(row[9]) * monthRatio);
                        var K = (int)(Convert.ToDouble(row[10]) * monthRatio);
                        var L = (int)(Convert.ToDouble(row[11]) * monthRatio);
                        var M = (int)(Convert.ToDouble(row[12]) * monthRatio);

                        int sheetId = managers.First(x => x.Item2 == (string)row[0]).Item1;

                        requestContainer.Add(GetRowRequest(sheetId, GetCellData(A, B, C, D, E, H, I, J, K, L, M)));
                    }
                }
            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }

        private async Task ProcessManager((int, string) manager, (int, int) dataRange)
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

            //Список закрытых сделок
            _processQueue.UpdateTaskName($"{_taskName}_{manager.Item2}", $"WeeklyReport: {dates}, closed leads");

            var allLeads = _leadRepo.GetByCriteria($"filter[pipeline_id][0]=3198184&filter[closed_at][from]={dataRange.Item1}&filter[closed_at][to]={dataRange.Item2}&filter[responsible_user_id]={manager.Item1}");

            //Количество закрытых сделок
            int finishedLeads = allLeads.Where(x => (x.status_id == 142) || (x.status_id == 143)).Count();

            //Количество успешных сделок
            int successLeads = allLeads.Where(x => x.status_id == 142).Count();

            //Список звонков
            _processQueue.UpdateTaskName($"{_taskName}_{manager.Item2}", $"WeeklyReport: {dates}, getting calls");
            Calls calls = new(dataRange, _contRepo, manager.Item1); 

            //Количество исходящих вызовов
            int outCallsCount = calls.outCalls.Count();

            //Количество входящих вызовов
            int inCallsCount = calls.inCalls.Count();

            //Количество пропущенных вызовов
            _processQueue.UpdateTaskName($"{_taskName}_{manager.Item2}", $"WeeklyReport: {dates}, calculating missed calls");

            int missedCallsCount = 0;

            var callIdList = new List<int>();

            foreach (var e in calls.inCalls)
                callIdList.Add(e.value_after[0].note.id);

            Parallel.ForEach(
                _contRepo.BulkGetNotesById(callIdList),
                new ParallelOptions { MaxDegreeOfParallelism = 3 },
                n =>
            {
                int duration = -1;

                if (n.parameters is not null)
                    duration = (int)n.parameters.duration;

                if (duration == 0) missedCallsCount++;
            });

            //Всего продаж
            int totalSales = allLeads.Where(x => x.status_id == 142).Sum(n => (int)n.price);

            //Цикл сделки
            double averageTime = 0;
            if (finishedLeads > 0)
                averageTime = allLeads.AsParallel()
                    .Where(x => (x.status_id == 142) || (x.status_id == 143))
                    .Select(x => (int)x.closed_at - (int)x.created_at).Average() / 86400;

            List<Request> requestContainer = new();

            requestContainer.Add(GetRowRequest(manager.Item1, GetCellData(dates, totalNewLeads, finishedLeads, successLeads, totalSales, averageTime, responseTime, longLeads, inCallsCount, outCallsCount, missedCallsCount)));

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);

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
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Время ответа, сек" } }
                        }
                });
                #endregion

                foreach (var l in leads)
                {
                    rows.Add(new RowData()
                    {
                        Values = new List<CellData>(){
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ FormulaValue = $@"=HYPERLINK(""https://mzpoeducationsale.amocrm.ru/leads/detail/{l.Item2}"", ""{l.Item2}"")" } },
                         new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = $"{l.Item3}" } }
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
                            StartRowIndex = dataRanges.Count + 3,
                            EndRowIndex = dataRanges.Count + 3 + rows.Count,
                            StartColumnIndex = 0,
                            EndColumnIndex = 2
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

            CalculateDateRange();

            await PrepareSheets();

            await AddKPIData();

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