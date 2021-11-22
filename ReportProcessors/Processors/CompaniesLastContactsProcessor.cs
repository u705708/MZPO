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
    internal class CompaniesLastContactsProcessor : AbstractReportProcessor, IReportProcessor
    {
        #region Definition

        /// <summary>
        /// Формирует отчёт для корпоративного отдела. Выгружает список компаний и дату последнего контакта.
        /// </summary>
        internal CompaniesLastContactsProcessor(AmoAccount acc, ProcessQueue processQueue, GSheets gSheets, string spreadsheetId, long dateFrom, long dateTo, string taskName, CancellationToken token)
            : base(acc, processQueue, gSheets, spreadsheetId, dateFrom, dateTo, taskName, token) { }
        #endregion

        #region Supplementary methods
        private async Task PrepareSheets()
        {
            #region Retrieving spreadsheet
            List<Request> requestContainer = new();
            var spreadsheet = _service.Spreadsheets.Get(_spreadsheetId).Execute();
            #endregion

            #region Adding temp sheet
            requestContainer.Add(new Request() {
                AddSheet = new AddSheetRequest() {
                    Properties = new SheetProperties() {
                        GridProperties = new GridProperties() {
                            ColumnCount = 5,
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
                if (s.Properties.SheetId == 31337)
                    continue;
                requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = s.Properties.SheetId } });
            }
            #endregion

            #region Creating CellFormat for alignment
            var centerAlignment = new CellFormat() {
                TextFormat = new TextFormat() {
                    Bold = true,
                    FontSize = 11
                },
                HorizontalAlignment = "CENTER",
                VerticalAlignment = "MIDDLE"
            };
            #endregion

            #region Adding sheet
            requestContainer.Add(new Request() {
                AddSheet = new AddSheetRequest() {
                    Properties = new SheetProperties() {
                        GridProperties = new GridProperties() {
                            ColumnCount = 6,
                            FrozenRowCount = 1
                        },
                        Title = "Компании",
                        SheetId = 0
                    }
                }
            });
            #endregion

            #region Adding header
            requestContainer.Add(new Request() {
                UpdateCells = new UpdateCellsRequest() {

                    Fields = "*",
                    Start = new GridCoordinate() { ColumnIndex = 0, RowIndex = 0, SheetId = 0 },
                    Rows = new List<RowData>() { new RowData() { Values = new List<CellData>(){
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "ID компании"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Название"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Email"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Последний контакт"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Ответственный"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Есть закрытая сделка?"} },
                            } }
                        }
                }
            });
            #endregion

            #region Adjusting column width
            var width = new List<int>() { 116, 600, 202, 168, 180, 144 };
            int i = 0;

            foreach (var c in width)
            {
                requestContainer.Add(new Request() {
                    UpdateDimensionProperties = new UpdateDimensionPropertiesRequest() {
                        Fields = "PixelSize",
                        Range = new DimensionRange() { SheetId = 0, Dimension = "COLUMNS", StartIndex = i, EndIndex = i + 1 },
                        Properties = new DimensionProperties() { PixelSize = c }
                    }
                });
                i++;
            }
            #endregion

            #region Delete temp sheet
            requestContainer.Add(new Request() { DeleteSheet = new DeleteSheetRequest() { SheetId = 31337 } });
            #endregion

            await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
        }

        private static CellData[] GetCellData(int A, string B, string C, string D, string E, bool F)
        {
            string Ft = F ? "Да" : "Нет";

            return new[]{
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ FormulaValue = $@"=HYPERLINK(""https://mzpoeducation.amocrm.ru/companies/detail/{A}"", ""{A}"")" },
                    UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = B},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = C},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = D},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = E},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                new CellData(){
                    UserEnteredValue = new ExtendedValue(){ StringValue = Ft},
                    UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
            };
        }

        private static string GetManager(int id)
        {
            return managersCorp.Any(x => x.Item1 == id) ? managersCorp.First(x => x.Item1 == id).Item2 : id.ToString();
        }

        private static bool CheckEventsRecent(List<(string, long)> events, DateTime refDT, out long lastContactEventTime)
        {
            lastContactEventTime = 0;

            if (!events.Any(e => e.Item1 == "outgoing_chat_message" ||
                                 e.Item1 == "incoming_chat_message" ||
                                 e.Item1 == "outgoing_call" ||
                                 e.Item1 == "incoming_call"))
                return false;

            lastContactEventTime = events.Where(e => e.Item1 == "outgoing_chat_message" ||
                                                     e.Item1 == "incoming_chat_message" ||
                                                     e.Item1 == "outgoing_call" ||
                                                     e.Item1 == "incoming_call")
                                             .Select(x => x.Item2)
                                             .Max();

            return DateTimeOffset.FromUnixTimeSeconds(lastContactEventTime).UtcDateTime.AddHours(3) > refDT;
        }

        private static bool CheckNotesRecent(List<(string, long)> notes, DateTime refDT, out long lastNoteEventTime)
        {
            lastNoteEventTime = 0;

            if (!notes.Any(n => n.Item1 == "amomail_message"))
                return false;

            lastNoteEventTime = notes.Where(n => n.Item1 == "amomail_message")
                                     .Select(x => x.Item2)
                                     .Max();

            return DateTimeOffset.FromUnixTimeSeconds(lastNoteEventTime).UtcDateTime.AddHours(3) > refDT;
        }

        private static bool CheckLeadRecent(Lead lead, DateTime refDT, out long leadCreatedTime, out bool completed)
        {
            leadCreatedTime = 0;

            completed = lead.status_id == 142 || lead.status_id == 35001244 || lead.status_id == 19529785;

            if (lead.created_at is null)
                return false;

            leadCreatedTime = (long)lead.created_at;

            return DateTimeOffset.FromUnixTimeSeconds(leadCreatedTime).UtcDateTime.AddHours(3) > refDT;
        }

        private static bool CheckCompanyRecent(Company company, DateTime refDT, out long companyCreatedTime)
        {
            companyCreatedTime = 0;

            if (company.created_at is null)
                return false;

            companyCreatedTime = (long)company.created_at;

            return DateTimeOffset.FromUnixTimeSeconds(companyCreatedTime).UtcDateTime.AddHours(3) > refDT;
        }

        private static IEnumerable<Company> GetCompanies(IAmoRepo<Company> compRepo)
        {
            List<(int, int)> dataranges = new()
{
                //(1541001600, 1543593600),   //test
                (1525104000, 1541001599),   //05.2018-10.2018
                (1541001600, 1572537599),   //11.2018-10.2019
                (1572537600, 1588262399),   //11.2019-04.2020
                (1588262400, 1604159999),   //05.2020-10.2020
                (1604160000, 1619798399),   //11.2020-04.2021
                (1619798400, 1635695999),   //05.2021-10.2021
                (1635696000, 1651265999),   //11.2021-04.2022
            };

            IEnumerable<Company> companies = null;

            foreach (var d in dataranges)
            {
                if (companies is null)
                {
                    companies = compRepo.GetByCriteria($"filter[created_at][from]={d.Item1}&filter[created_at][to]={d.Item2}&with=contacts,leads");
                    continue;
                }

                companies = companies.Concat(compRepo.GetByCriteria($"filter[created_at][from]={d.Item1}&filter[created_at][to]={d.Item2}&with=contacts,leads"));
            }

            return companies;
        }

        private static IEnumerable<Company> GetAllCompanies(IAmoRepo<Company> compRepo)
        {
            var startPeriod = new DateTime(2018, 05, 01, 0, 0, 0, DateTimeKind.Utc);
            var endPeriod = startPeriod.AddMonths(1).AddSeconds(-1);
            var endDate = DateTime.UtcNow;

            IEnumerable<Company> companies = compRepo.GetByCriteria($"filter[created_at][from]={((DateTimeOffset)startPeriod).ToUnixTimeSeconds()}&filter[created_at][to]={((DateTimeOffset)endPeriod).ToUnixTimeSeconds()}&with=contacts,leads");

            while (startPeriod < endDate)
            {
                startPeriod = startPeriod.AddHours(3).AddMonths(1).AddHours(-3);
                endPeriod = endPeriod.AddSeconds(1).AddHours(3).AddMonths(1).AddHours(-3).AddSeconds(-1);

                companies = companies.Concat(compRepo.GetByCriteria($"filter[created_at][from]={((DateTimeOffset)startPeriod).ToUnixTimeSeconds()}&filter[created_at][to]={((DateTimeOffset)endPeriod).ToUnixTimeSeconds()}&with=contacts,leads"));
            }

            return companies;
        }

        private Request ProcessResult(int compId, string companyName, string email, string lastContact, int respManager, bool openLead)
        {
            return GetRowRequest(0, GetCellData(compId, companyName, email, lastContact, GetManager(respManager), openLead));
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
            }
            catch
            {
                _processQueue.Remove(_taskId);
                return;
            }

            List<(int compId, string companyName, string email, string lastContact, int respManager, bool openLead)> resultsList = new();
            DateTime referenceDateTime = DateTime.UtcNow.AddHours(3).AddMonths(-6);
            var companies = GetAllCompanies(_compRepo);

            int i = 0;

            Parallel.ForEach(
                companies,
                new ParallelOptions { MaxDegreeOfParallelism = 12 },
                c => {
                    i++;

                    if (i % 60 == 0)
                    {
                        GC.Collect();
                        _processQueue.UpdateTaskName(_taskId, $"Reports. CompaniesLastContacts: processed {i} leads");
                    }

                    List<long> timeStamps = new();
                    long contactTime = 0;
                    bool completed = false;

                    string email = c.GetCFStringValue(33577);

                    //Для отчёта по email компаний, чтобы не обрабатывать компании без почты
                    //if (string.IsNullOrEmpty(email))
                    //    return;

                    #region Collecting company notes and events
                    CheckCompanyRecent(c, referenceDateTime, out contactTime);
                    timeStamps.Add(contactTime);

                    CheckEventsRecent(_compRepo.GetEntityEvents(c.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
                    timeStamps.Add(contactTime);

                    CheckNotesRecent(_compRepo.GetEntityNotes(c.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
                    timeStamps.Add(contactTime);
                    #endregion

                    #region Collecting associated leads notes and events
                    if (c._embedded.leads is not null)
                        foreach (var lead in c._embedded.leads.OrderByDescending(x => x.id))
                        {
                            CheckLeadRecent(_leadRepo.GetById(lead.id), referenceDateTime, out contactTime, out completed);
                            timeStamps.Add(contactTime);

                            CheckEventsRecent(_leadRepo.GetEntityEvents(lead.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
                            timeStamps.Add(contactTime);

                            CheckNotesRecent(_leadRepo.GetEntityNotes(lead.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
                            timeStamps.Add(contactTime);
                        }
                    #endregion

                    #region Collecting associated contacts notes and events
                    if (c._embedded.contacts is not null)
                        foreach (var contact in c._embedded.contacts.OrderByDescending(x => x.id))
                        {
                            CheckEventsRecent(_contRepo.GetEntityEvents((int)contact.id).Select(x => (x.type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
                            timeStamps.Add(contactTime);

                            CheckNotesRecent(_contRepo.GetEntityNotes((int)contact.id).Select(x => (x.note_type, (long)x.created_at)).ToList(), referenceDateTime, out contactTime);
                            timeStamps.Add(contactTime);
                        }
                    #endregion

                    var lastContactTime = DateTimeOffset.FromUnixTimeSeconds(timeStamps.Max()).UtcDateTime.AddHours(3);

                    resultsList.Add(
                        (c.id,
                         c.name.Replace(";", " ").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "),
                         email,
                         $"{lastContactTime.ToShortDateString()} {lastContactTime.ToShortTimeString()}",
                         (int)c.responsible_user_id,
                         completed
                        ));
                });

            List<Request> requestContainer = new();

            foreach (var r in resultsList)
            {
                if (_token.IsCancellationRequested)
                    break;

                var result = ProcessResult(r.compId, r.companyName, r.email, r.lastContact, r.respManager, r.openLead);
                if (result is not null)
                    requestContainer.Add(result);
            }

            try
            {
                await UpdateSheetsAsync(requestContainer, _service, _spreadsheetId);
            }
            finally
            {
                _processQueue.Remove(_taskId);
            }
        }
        #endregion
    }
}