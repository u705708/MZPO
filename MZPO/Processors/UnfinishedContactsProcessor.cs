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
    public class UnfinishedContactsProcessor : IProcessor
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

        public UnfinishedContactsProcessor(AmoAccount acc, GSheets gSheets, string spreadsheetId, TaskList processQueue, CancellationToken token)
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
            (2375122, "Васина Елена"),
            (2375116, "Киреева Светлана"),
            (2375131, "Алферова Лилия"),
            (6630727, "Елена Зубатых"),
            (6028753, "Алена Федосова"),
            (2884132, "Ирина Сорокина"),
            (3770773, "Шталева Лидия"),
            (6200629, "Харшиладзе Леван"),
            (6346882, "Мусихина Юлия")
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
                                ColumnCount = 6,
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
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Номер сделки"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Название контакта"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Название компании"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Есть телефоны"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "Есть email?"} },
                            new CellData(){ UserEnteredFormat = centerAlignment, UserEnteredValue = new ExtendedValue() { StringValue = "ЛПР"} }
                            } }
                        }
                    }
                });
                #endregion

                #region Adjusting column width
                var width = new List<int>() { 116, 144, 300, 124, 92, 200 };
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

        private void PrepareRow(int sheetId, int A, string B, string C, bool D, bool E, string F)
        {
            #region Prepare data
            var rows = new List<RowData>();

            string Dt = D ? "Да" : "";
            string Et = E ? "Да" : "";

            rows.Add(new RowData()
            {
                Values = new List<CellData>(){
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = A},
                             UserEnteredFormat = new CellFormat(){ HorizontalAlignment = "CENTER", NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = B},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = C},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = Dt},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = Et},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = F},
                             UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
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
            #region Preparing fields
            int leadNumber = l.id;
            string contactName;
            string companyName;
            bool phoneAdded = false;
            bool emailAdded = false;
            string LPR = "";

            Company company;
            List<Contact> contacts = new List<Contact>();
            #endregion

            #region Processing associated company
            if (l._embedded.companies.Any())
                company = compRepo.GetById(l._embedded.companies.FirstOrDefault().id);
            else
                company = new Company() { name = $"Сделка {leadNumber} без компании" };

            companyName = company.name;

            #region Getting LPR
            if ((company.custom_fields_values != null) &&
                company.custom_fields_values.Any(x => x.field_id == 640657))
                LPR = (string)company.custom_fields_values.FirstOrDefault(x => x.field_id == 640657).values[0].value;
            #endregion

            #region Checking company for contacts
            if ((company.custom_fields_values != null) &&
                company.custom_fields_values.Any(x => x.field_id == 33575))
                phoneAdded = true;
            if ((company.custom_fields_values != null) &&
                company.custom_fields_values.Any(x => x.field_id == 33577))
                emailAdded = true;
            #endregion
            #endregion

            #region Collecting associated contacts

            var contactIdList = new List<int>();
            var criteria = new StringBuilder("");

            if ((l._embedded.contacts != null) &&
                (l._embedded.contacts.Any()))
                foreach (var c in l._embedded.contacts)
                    contactIdList.Add(c.id); 
            if ((company._embedded != null) &&
                (company._embedded.contacts != null) &&
                (company._embedded.contacts.Any()))
                foreach (var c in company._embedded.contacts)
                    contactIdList.Add(c.id);

            contacts.AddRange(contRepo.BulkGetById(contactIdList));
            #endregion

            #region Getting contact name if any
            if ((contacts != null) &&
                contacts.Any())
                contactName = contacts.First().name;
            else contactName = "";
            #endregion

            #region Checking contacts
            if ((contacts != null) && 
                contacts.Any())
                foreach (var c in contacts)
                {
                    if (c.custom_fields_values == null) continue;
                    if (c.custom_fields_values.Any(x => x.field_id == 33575))
                        phoneAdded = true;
                    if (c.custom_fields_values.Any(x => x.field_id == 33577))
                        emailAdded = true;
                }
            #endregion

            #region Preparing row if needed
            if (!phoneAdded || !emailAdded)
                PrepareRow(sheetId, leadNumber, contactName, companyName, phoneAdded, emailAdded, LPR);
            #endregion
        }

        private void ProcessManager((int, string) m)
        {
            requestContainer = new List<Request>();

            #region Preparing criteria for amo requests
            List<string> criteria = new List<string>()
                {
                    $"filter[statuses][0][pipeline_id]=1121263&filter[statuses][0][status_id]=142&filter[responsible_user_id]={m.Item1}&with=companies,contacts",
                    $"filter[statuses][0][pipeline_id]=1121263&filter[statuses][0][status_id]=19529785&filter[responsible_user_id]={m.Item1}&with=companies,contacts",
                    $"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=142&filter[responsible_user_id]={m.Item1}&with=companies,contacts",
                    $"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[responsible_user_id]={m.Item1}&with=companies,contacts"
                };
            #endregion

            _processQueue.UpdateTaskName("report_data", $"Unfinished Companies report: {m.Item2}");

            foreach (var cr in criteria)
            {
                var allLeads = leadRepo.GetByCriteria(cr);

                int counter = 0;

                if (allLeads == null) continue;

                foreach (var l in allLeads)
                {
                    if (_token.IsCancellationRequested) break;

                    if (counter % 10 == 0)
                        _processQueue.UpdateTaskName("report_data", $"Unfinished Companies report: {m.Item2}, {counter}/{allLeads.Count()}, {cr}");
                    ProcessLead(l, m.Item1);
                    counter++;
                }
                GC.Collect();
            }

            #region Remove duplicates
            if (requestContainer.Any())
            {
                requestContainer.Add(new Request()
                {
                    DeleteDuplicates = new DeleteDuplicatesRequest()
                    {
                        Range = new GridRange()
                        {
                            SheetId = m.Item1,
                            StartColumnIndex = 0,
                            EndColumnIndex = 6,
                            StartRowIndex = 1,
                            EndRowIndex = requestContainer.Count + 2
                        },
                        ComparisonColumns = new List<DimensionRange>() { new DimensionRange()
                    {
                        SheetId = m.Item1,
                        Dimension = "COLUMNS",
                        StartIndex = 2,
                        EndIndex = 3
                    } }
                    }
                });
            }
            #endregion

            #region Updating sheet
            if (requestContainer.Any())
            {
                var batchRequest = new BatchUpdateSpreadsheetRequest();
                batchRequest.Requests = requestContainer;

                _service.Spreadsheets.BatchUpdate(batchRequest, SpreadsheetId).Execute();
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

            Log.Add("Started Unfinished Companies report");

            PrepareSheets();

            foreach (var m in managers)
            {
                if (_token.IsCancellationRequested) break;

                ProcessManager(m);
            }

            Log.Add("Finished Unfinished Companies report");
            _processQueue.Remove("report_data");
        }
        #endregion
    }
}