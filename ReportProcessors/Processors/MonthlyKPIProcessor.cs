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
    internal class MonthlyKPIProcessor : AbstractReportProcessor, IReportProcessor
    {
        #region Definition
        private readonly (int, int) dataRange;

        /// <summary>
        /// Формирует отчёт KPI для отдела розницы. Собирает показатели по каждому менеджеру по итогам месяца.
        /// </summary>
        internal MonthlyKPIProcessor(AmoAccount acc, ProcessQueue processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
            dataRange = ((int)dateFrom, (int)dateTo);

            _longAnsweredLeads = new();
        }

        private readonly List<(int, string)> _specials = new();
        private readonly List<(int, string)> _newProducts = new();

        private readonly Dictionary<string, CellFormat> columns = new()
        {
            { "A", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "B", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "C", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "D", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "E", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "F", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "G", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "H", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "I", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ### сек" } } },
            { "J", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "K", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "L", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } },
            { "M", new CellFormat() { HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "NUMBER", Pattern = "# ### ###" } } }
        };
        #endregion

        class LeadComparer : IEqualityComparer<Lead>
        {
            public bool Equals(Lead x, Lead y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (x is null || y is null)
                    return false;

                return x.id == y.id;
            }

            public int GetHashCode(Lead c)
            {
                if (c is null) return 0;

                int hashProductCode = c.id.GetHashCode();

                return hashProductCode;
            }
        }

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
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Закрытых сделок"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "из них очные"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "дистанционные"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "повторные"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Кол-во исходящих"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "первых звонков"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "прочих"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Длительность исходящих, сек"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Отправлено КП"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Переведено из акутализации"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Акций"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Новых продуктов"} }
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 156, 156, 132, 144, 96, 156, 144, 72, 240, 132, 228, 60, 144 };
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

        private CellData[] GetCellData(string A, int B, int C, int D, int E, int F, int G, int I, int J, int K, int L, int M)
        {
            return new[]{
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
                    UserEnteredValue = new ExtendedValue(){ NumberValue = F},
                    UserEnteredFormat = columns["F"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = G},
                    UserEnteredFormat = columns["G"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = @"=INDIRECT(""R[0]C[-2]"", FALSE)-INDIRECT(""R[0]C[-1]"", FALSE)"},
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
            };
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

            foreach (var m in managersRet)
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

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }

        private async Task ProcessManager((int, string) manager)
        {
            //Даты
            string dates = $"{DateTimeOffset.FromUnixTimeSeconds(dataRange.Item1).UtcDateTime.AddHours(3).ToShortDateString()} - {DateTimeOffset.FromUnixTimeSeconds(dataRange.Item2).UtcDateTime.AddHours(3).ToShortDateString()}";

            //Список закрытых сделок
            _processQueue.AddSubTask(_taskId, $"{_taskId}_{manager.Item2}", $"KPIReport: {dates}, closed leads");

            List<Lead> allLeads = new();

            allLeads.AddRange(_leadRepo.GetByCriteria($"filter[pipeline_id][0]=3198184&filter[closed_at][from]={dataRange.Item1}&filter[closed_at][to]={dataRange.Item2}&filter[responsible_user_id]={manager.Item1}&with=contacts"));

            //Всего продаж
            int totalSales = allLeads.Where(x => x.status_id == 142)
                                     .Sum(x => (int)x.price);

            //Продаж очных сделок
            int fulltimeSales = allLeads.Where(x => x.status_id == 142)
                                        .Where(x => x.custom_fields_values is not null &&
                                                    x.custom_fields_values.Any(y => y.field_id == 643207))
                                        .Where(x => (string)x.custom_fields_values.First(y => y.field_id == 643207).values[0].value == "Очная" ||
                                                    (string)x.custom_fields_values.First(y => y.field_id == 643207).values[0].value == "ОчноЗаочная" ||
                                                    (string)x.custom_fields_values.First(y => y.field_id == 643207).values[0].value == "ОчноДистанционная")
                                        .Sum(x => (int)x.price);

            //Продаж дистанционных сделок
            int distantSales = allLeads.Where(x => x.status_id == 142)
                                       .Where(x => x.custom_fields_values is not null &&
                                                   x.custom_fields_values.Any(y => y.field_id == 643207))
                                       .Where(x => (string)x.custom_fields_values.First(y => y.field_id == 643207).values[0].value == "Дистанционная" ||
                                                   (string)x.custom_fields_values.First(y => y.field_id == 643207).values[0].value == "Вебинар онлайн")
                                       .Sum(x => (int)x.price);

            //Кол-во повторных продаж
            _processQueue.UpdateTaskName($"{_taskId}_{manager.Item2}", $"WeeklyReport: {dates}, recurrent leads");

            int recurrentLeads = 0;
            
            foreach (var l in allLeads.Where(x => x.status_id == 142))
            {
                if (l._embedded is not null &&
                    l._embedded.contacts is not null)
                {
                    List<Lead> connectedContactsLeads = new();

                    foreach (var c in l._embedded.contacts)
                    {
                        var contact = _contRepo.GetById((int)c.id);
                        if (contact._embedded is not null &&
                            contact._embedded.leads is not null)
                            connectedContactsLeads.AddRange(contact._embedded.leads);
                    }

                    if (connectedContactsLeads.Distinct(new LeadComparer()).Count() > 1)
                        recurrentLeads++;
                }
            }

            //Список звонков
            _processQueue.UpdateTaskName($"{_taskId}_{manager.Item2}", $"WeeklyReport: {dates}, getting calls");
            Calls calls = new(dataRange, _contRepo, manager.Item1);

            //Количество исходящих вызовов
            int outCallsCount = calls.outCalls.Count();

            //Длительность исходящих вызовов
            _processQueue.UpdateTaskName($"{_taskId}_{manager.Item2}", $"KPIReport: {dates}, calls duration");

            var callIdList = new List<int>();

            foreach (var e in calls.outCalls)
                callIdList.Add(e.value_after[0].note.id);

            var callNotes = _contRepo.BulkGetNotesById(callIdList);

            int callsDuration = callNotes.Where(x => x.parameters is not null)
                                         .Sum(x => (int)x.parameters.duration);

            //Количество первых вызовов
            _processQueue.UpdateTaskName($"{_taskId}_{manager.Item2}", $"KPIReport: {dates}, first calls");

            int firstCallsCount = 0; //------------

            Parallel.ForEach(
                calls.outCalls,
                new ParallelOptions { MaxDegreeOfParallelism = 3 },
                c => {
                    var allContactCalls = _contRepo.GetEntityEvents((int)c.entity_id);
                    if (allContactCalls.Any(x => x.type == "outgoing_call" || x.type == "incoming_call") &&
                        allContactCalls.Where(x => x.type == "outgoing_call" || x.type == "incoming_call").OrderBy(x => x.created_at).First().id == c.id)
                        firstCallsCount++;
            });

            //Отправлено КП
            _processQueue.UpdateTaskName($"{_taskId}_{manager.Item2}", $"KPIReport: {dates}, KP sent");

            var criteria = $"filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&filter[created_by][]={manager.Item1}&filter[entity][]=lead&filter[type][]=lead_status_changed&filter[value_after][leads_statuses][0][pipeline_id]=3198184&filter[value_after][leads_statuses][0][status_id]=32532886";
            List<Event> sentKPEvents = new();
            sentKPEvents.AddRange(_leadRepo.GetEventsByCriteria(criteria));
            int sentKPCount = sentKPEvents.Count;

            //Переведено из актуализации
            _processQueue.UpdateTaskName($"{_taskId}_{manager.Item2}", $"KPIReport: {dates}, Actualization");

            criteria = $"filter[created_at][from]={dataRange.Item1}&filter[created_at][to]={dataRange.Item2}&filter[created_by][]={manager.Item1}&filter[entity][]=lead&filter[type][]=lead_status_changed&filter[value_before][leads_statuses][0][pipeline_id]=3558922&filter[value_before][leads_statuses][0][status_id]=35002129";
            List<Event> actualizationCheckEvents = new();
            actualizationCheckEvents.AddRange(_leadRepo.GetEventsByCriteria(criteria));
            int actualizationCheckCount = actualizationCheckEvents.Count;

            //Акции
            var specialsLeads = allLeads.Where(x => x.status_id == 142)
                                       .Where(x => x.custom_fields_values is not null &&
                                                   x.custom_fields_values.Any(y => y.field_id == 709753))
                                       .ToList();
            int specialsLeadsCount = specialsLeads.Count;

            _specials.AddRange(specialsLeads.Select(x => (manager.Item1, (string)x.custom_fields_values.First(x => x.field_id == 709753).values[0].value)));

            //Новые продукты
            var newProductLeads = allLeads.Where(x => x.status_id == 142)
                                          .Where(x => x.custom_fields_values is not null &&
                                                      x.custom_fields_values.Any(y => y.field_id == 709755))
                                          .ToList();
            int newProductLeadsCount = newProductLeads.Count;

            _newProducts.AddRange(newProductLeads.Select(x => (manager.Item1, (string)x.custom_fields_values.First(x => x.field_id == 709755).values[0].value)));


            //Сохраняем результаты
            List<Request> requestContainer = new();
            requestContainer.Add(GetRowRequest(manager.Item1, GetCellData(dates, totalSales, fulltimeSales, distantSales, recurrentLeads, outCallsCount, firstCallsCount, callsDuration, sentKPCount, actualizationCheckCount, specialsLeadsCount, newProductLeadsCount)));

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);

            _processQueue.Remove($"{_taskId}_{manager.Item2}");
        }

        private async Task FinalizeManagers()
        {
            List<Request> requestContainer = new();

            foreach (var m in managersRet)
            {
                #region Prepare Data
                var rows = new List<RowData>();

                #region Header
                rows.Add(new RowData()
                {
                    Values = new List<CellData>(){
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Акция" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Количество" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                    }
                });
                #endregion

                var specials = _specials.Where(x => x.Item1 == m.Item1).GroupBy(x => x.Item2).Select(g => new { Item = g.Key, Count = g.Count() }); 

                foreach (var s in specials)
                {
                    rows.Add(new RowData()
                    {
                        Values = new List<CellData>(){
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = $"{s.Item}" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ NumberValue = s.Count } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                        }
                    });
                }

                #region Header
                rows.Add(new RowData()
                {
                    Values = new List<CellData>(){
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Новые продукты" } },
                        new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "Количество" } },
                    }
                });
                #endregion

                var newProducts = _newProducts.Where(x => x.Item1 == m.Item1).GroupBy(x => x.Item2).Select(g => new { Item = g.Key, Count = g.Count() });

                foreach (var n in newProducts)
                {
                    rows.Add(new RowData()
                    {
                        Values = new List<CellData>(){
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = "" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ StringValue = $"{n.Item}" } },
                            new CellData(){ UserEnteredValue = new ExtendedValue(){ NumberValue = n.Count } },
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
                            StartRowIndex = 4,
                            EndRowIndex = 4 + rows.Count,
                            StartColumnIndex = 0,
                            EndColumnIndex = 4
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
                _processQueue.Remove(_taskId);
                return;
            }

            await PrepareSheets();

            List<Task> tasks = new();

            foreach (var manager in managersRet)
            {
                if (_token.IsCancellationRequested) break;
                var m = manager;
                tasks.Add(Task.Run(() => ProcessManager(m)));
            }

            await Task.WhenAll(tasks);

            await FinalizeManagers();

            _processQueue.Remove(_taskId);
        }
        #endregion
    }
}