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
    internal class LeadscompletionReportProcessor : AbstractReportProcessor, IReportProcessor
    {
        #region Definition
        private readonly (int, int) dataRange;

        /// <summary>
        /// Формирует отчёт для отдела розницы, собирает показатели с начала месяца, сравнивает со среднемесячными показателями за аналогичный период.
        /// </summary>
        internal LeadscompletionReportProcessor(AmoAccount acc, ProcessQueue processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token)
        {
            dataRange = ((int)dateFrom, (int)dateTo);
        }

        private readonly Dictionary<string, CellFormat> columnsFormat = new()
        {
            { "A", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "B", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "C", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "D", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "E", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "F", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "G", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "H", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "I", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "J", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "K", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "L", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            { "M", new CellFormat() { NumberFormat = new NumberFormat() { Type = "TEXT" } } },
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
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Номер сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Название сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Курс"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Цена"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Срок обучения"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Форма обучения"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Тип обучения"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Начало обучения"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Дата экзамена"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Имя"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Телефон"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "email"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Ответственный"} }
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 120, 144, 96, 72, 120, 144, 120, 144, 120, 180, 108, 180, 168 };
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

        private CellData[] GetCellData(int A, string B, string C, int D, string E, string F, string G, string H, string I, string J, string K, string L, string M)
        {
            return new[]{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = $@"=HYPERLINK(""https://mzpoeducationsale.amocrm.ru/leads/detail/{A}"", ""{A}"")" },
                    UserEnteredFormat = columnsFormat["A"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = B},
                    UserEnteredFormat = columnsFormat["B"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = C},
                    UserEnteredFormat = columnsFormat["C"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ NumberValue = D},
                    UserEnteredFormat = columnsFormat["D"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = E},
                    UserEnteredFormat = columnsFormat["E"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = F},
                    UserEnteredFormat = columnsFormat["F"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = G},
                    UserEnteredFormat = columnsFormat["G"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = H},
                    UserEnteredFormat = columnsFormat["H"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = I},
                    UserEnteredFormat = columnsFormat["I"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = J},
                    UserEnteredFormat = columnsFormat["J"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = K},
                    UserEnteredFormat = columnsFormat["K"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = L},
                    UserEnteredFormat = columnsFormat["L"] },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = M},
                    UserEnteredFormat = columnsFormat["M"] },
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

        private async Task ProcessManagers()
        {
            var successLeads = _leadRepo
                                .GetByCriteria($"filter[pipeline_id][0]=3198184&filter[closed_at][from]={dataRange.Item1}&filter[closed_at][to]={dataRange.Item2}&with=contacts")
                                .Where(x => x.status_id == 142);

            List<Request> requestContainer = new();

            int i = 0;

            Parallel.ForEach(
                successLeads,
                new ParallelOptions { MaxDegreeOfParallelism = 8 },
                l => {
                    i++;
                    
                    int leadId = l.id;

                    string leadName = l.name;

                    string courseName = l.GetCFStringValue(357005);

                    int leadPrice = l.price is null ? 0 : (int)l.price;

                    string educationLength = l.GetCFStringValue(644757);

                    string educationForm = l.GetCFStringValue(643207);

                    string educationType = l.GetCFStringValue(644763);

                    string educationStart = "";
                    if (l.custom_fields_values is not null &&
                        l.custom_fields_values.Any(x => x.field_id == 643199))
                    {
                        long date = (long)l.custom_fields_values.First(x => x.field_id == 643199).values[0].value;
                        educationStart = $"{DateTimeOffset.FromUnixTimeSeconds(date).UtcDateTime.AddHours(3).ToShortDateString()}";
                    }

                    string examDate = "";
                    if (l.custom_fields_values is not null &&
                        l.custom_fields_values.Any(x => x.field_id == 644915))
                    {
                        long date = (long)l.custom_fields_values.First(x => x.field_id == 644915).values[0].value;
                        examDate = $"{DateTimeOffset.FromUnixTimeSeconds(date).UtcDateTime.AddHours(3).ToShortDateString()}";
                    }

                    List<string> contactNames = new();
                    List<string> contactPhones = new();
                    List<string> contactEmails = new();

                    if (l._embedded is not null &&
                        l._embedded.contacts is not null)
                        foreach (var c in l._embedded.contacts)
                        {
                            var contact = _contRepo.GetById((int)c.id);

                            contactNames.Add(contact.name);

                            if (contact.custom_fields_values is not null &&
                                contact.custom_fields_values.Any(x => x.field_id == 264911))
                                contactPhones.Add(contact.custom_fields_values.First(x => x.field_id == 264911).values[0].value.ToString());

                            if (contact.custom_fields_values is not null &&
                                contact.custom_fields_values.Any(x => x.field_id == 264913))
                                contactEmails.Add(contact.custom_fields_values.First(x => x.field_id == 264913).values[0].value.ToString());
                        }

                    string contactName = "";
                    if (contactNames.Any())
                        contactName = contactNames.OrderByDescending(x => x.Length).First();

                    string contactPhone = "";
                    if (contactPhones.Any())
                        contactPhone = contactPhones.First();

                    string contactEmail = "";
                    if (contactEmails.Any())
                        contactEmail = contactEmails.First();

                    string responsibleUser = "";
                    if (managersRet.Any(x => x.Item1 == l.responsible_user_id))
                        responsibleUser = managersRet.First(x => x.Item1 == l.responsible_user_id).Item2;
                    else return;

                    requestContainer.Add(GetRowRequest((int)l.responsible_user_id, GetCellData(leadId, leadName, courseName, leadPrice, educationLength, educationForm, educationType, educationStart, examDate, contactName, contactPhone, contactEmail, responsibleUser)));

                    if (i % 10 == 0)
                    {
                        GC.Collect();
                        _processQueue.UpdateTaskName(_taskId, $"report_cards_completion, processed {i} leads.");
                    }
                });

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

            try
            {
                await PrepareSheets();

                await ProcessManagers();

                _processQueue.Remove(_taskId);
            }
            catch
            {
                _processQueue.Remove(_taskId);
            }
        }
        #endregion
    }
}