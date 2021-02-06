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
    public class UnfinishedContactsProcessor : IProcessor
    {
        #region Definition
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly SheetsService _service;
        private readonly string SpreadsheetId;
        private readonly IAmoRepo<Lead> leadRepo;
        private readonly IAmoRepo<Company> compRepo;
        private readonly IAmoRepo<Contact> contRepo;
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

        private readonly List<(int, string)> managers = new List<(int, string)>
        {
            (2375122, "Васина Елена"),
            (2375116, "Киреева Светлана"),
            (2375131, "Алферова Лилия"),
            (6630727, "Елена Зубатых"),
            (6028753, "Алена Федосова"),
            (2884132, "Ирина Сорокина"),
            (6697522, "Наталья Филатова"),
            (3770773, "Шталева Лидия"),
            (6200629, "Харшиладзе Леван"),
            (6346882, "Мусихина Юлия")
        };
        #endregion

        #region Supplementary methods
        private async Task PrepareSheets()
        {
            #region Retrieving spreadsheet
            List<Request> requestContainer = new();
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

            await UpdateSheetsAsync(requestContainer);
        }

        private Request PrepareRow(int sheetId, int A, string B, string C, bool D, bool E, string F)
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

        private Request ProcessLead(Lead l, int sheetId)
        {
            #region Preparing fields
            int leadNumber = l.id;
            string contactName = "";
            string companyName;
            bool phoneAdded = false;
            bool emailAdded = false;
            string LPR = "";

            Company company;
            #endregion

            #region Processing associated company
            if (l._embedded.companies.Any())
                company = compRepo.GetById(l._embedded.companies.FirstOrDefault().id);
            else
                company = new Company() { name = $"Сделка {leadNumber} без компании" };

            companyName = company.name;

            #region Getting LPR
            if ((company.custom_fields_values is not null) &&
                company.custom_fields_values.Any(x => x.field_id == 640657))
                LPR = (string)company.custom_fields_values.FirstOrDefault(x => x.field_id == 640657).values[0].value;
            #endregion

            #region Checking company for contacts
            if ((company.custom_fields_values is not null) &&
                company.custom_fields_values.Any(x => x.field_id == 33575))
                phoneAdded = true;
            if ((company.custom_fields_values is not null) &&
                company.custom_fields_values.Any(x => x.field_id == 33577))
                emailAdded = true;
            #endregion
            #endregion

            #region Collecting associated contacts

            var contactIdList = new List<int>();

            if ((l._embedded.contacts is not null) &&
                (l._embedded.contacts.Any()))
                foreach (var c in l._embedded.contacts)
                    contactIdList.Add(c.id); 
            if ((company._embedded is not null) &&
                (company._embedded.contacts is not null) &&
                (company._embedded.contacts.Any()))
                foreach (var c in company._embedded.contacts)
                    contactIdList.Add(c.id);

            var contacts = contRepo.BulkGetById(contactIdList);
            #endregion

            #region Getting contact name if any
            if (contacts.Any())
                contactName = contacts.First().name;
            #endregion

            #region Checking contacts
            foreach (var c in contacts)
            {
                if (c.custom_fields_values is null) continue;
                if (c.custom_fields_values.Any(x => x.field_id == 33575))
                    phoneAdded = true;
                if (c.custom_fields_values.Any(x => x.field_id == 33577))
                    emailAdded = true;
            }
            #endregion

            #region Preparing row if needed
            if (!phoneAdded || !emailAdded)
                return PrepareRow(sheetId, leadNumber, contactName, companyName, phoneAdded, emailAdded, LPR);
            else return null;
            #endregion
        }

        private async Task ProcessManager((int, string) m)
        {
            List<Request> requestContainer = new();

            #region Preparing criteria for amo requests
            List<string> criteria = new List<string>()
                {
                    $"filter[statuses][0][pipeline_id]=1121263&filter[statuses][0][status_id]=142&filter[responsible_user_id]={m.Item1}&with=companies,contacts",
                    $"filter[statuses][0][pipeline_id]=1121263&filter[statuses][0][status_id]=19529785&filter[responsible_user_id]={m.Item1}&with=companies,contacts",
                    $"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=142&filter[responsible_user_id]={m.Item1}&with=companies,contacts",
                    $"filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[responsible_user_id]={m.Item1}&with=companies,contacts"
                };
            #endregion

            _processQueue.AddSubTask("report_data", $"report_data_{m.Item2}", $"Unfinished Companies report");

            List<Lead> allLeads = new();

            Parallel.ForEach(criteria, cr => { 
                var range = leadRepo.GetByCriteria(cr);
                lock (allLeads)
                {
                    allLeads.AddRange(range);
                }
            });

            _processQueue.UpdateTaskName($"report_data_{m.Item2}", $"Unfinished Companies report: total {allLeads.Count()} leads to check.");

            int counter = 0;

            foreach (var l in allLeads)
            {
                if (_token.IsCancellationRequested) break;

                if (counter % 10 == 0)
                    _processQueue.UpdateTaskName($"report_data_{m.Item2}", $"Unfinished Companies report: Processed {counter} of {allLeads.Count()} leads.");
                var result = ProcessLead(l, m.Item1);
                if (result is not null)
                    requestContainer.Add(result);
                counter++;
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

            await UpdateSheetsAsync(requestContainer);

            _processQueue.Remove($"report_data_{m.Item2}");
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
                _processQueue.Remove("report_data");
                return;
            }

            Log.Add("Started Unfinished Companies report");

            await PrepareSheets();

            List<Task> tasks = new();

            foreach (var manager in managers)
            {
                if (_token.IsCancellationRequested) break;
                var m = manager;
                tasks.Add(Task.Run(() => ProcessManager(m), _token));
            }

            await Task.WhenAll(tasks);

            Log.Add("Finished Unfinished Companies report");
            _processQueue.Remove("report_data");
        }
        #endregion
    }
}