using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class DODProcessor : ILeadProcessor
    {
        private readonly TaskList _processQueue;
        private readonly CancellationToken _token;
        private readonly Log _log;
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Contact> _contRepo;
        private readonly int _leadNumber;
        private readonly SheetsService _service;
        private readonly string _spreadsheetId;

        public DODProcessor(int leadnumber, Amo amo, GSheets gSheets, TaskList processQueue, Log log, CancellationToken token)
        {
            _processQueue = processQueue;
            _token = token;
            _log = log;
            _leadRepo = amo.GetAccountById(28395871).GetRepo<Lead>();
            _contRepo = amo.GetAccountById(28395871).GetRepo<Contact>();
            _leadNumber = leadnumber;
            _service = gSheets.GetService();
            _spreadsheetId = "1yOIbfo_8SqwkLkeNiQ_nb3UsitNsESupPCfyjzFwwnU";
        }

        public async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"DOD-{_leadNumber}");
                return;
            }
            try
            {
                Lead lead;
                try { lead = _leadRepo.GetById(_leadNumber); }
                catch { lead = null; }

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.contacts is null)
                    return;

                var leadId = lead.id.ToString();
                var date = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";

                Contact contact = _contRepo.GetById((int)lead._embedded.contacts.First().id);

                var name = contact.name;

                string phone = "";

                if (contact.custom_fields_values is not null &&
                    contact.custom_fields_values.Any(x => x.field_id == 264911))
                    phone = contact.custom_fields_values.First(x => x.field_id == 264911).values[0].value.ToString();

                string email = "";

                if (contact.custom_fields_values is not null &&
                    contact.custom_fields_values.Any(x => x.field_id == 264913))
                    email = contact.custom_fields_values.First(x => x.field_id == 264913).values[0].value.ToString();

                List<Request> requestContainer = new();

                var rows = new List<RowData>
                {
                    new RowData()
                    {
                        Values = new List<CellData>()
                        {
                            new CellData(){
                                UserEnteredValue = new ExtendedValue(){ StringValue = date},
                                UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                            new CellData(){
                                UserEnteredValue = new ExtendedValue(){ StringValue = leadId},
                                UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                            new CellData(){
                                UserEnteredValue = new ExtendedValue(){ StringValue = name},
                                UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                            new CellData(){
                                UserEnteredValue = new ExtendedValue(){ StringValue = phone},
                                UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                            new CellData(){
                                UserEnteredValue = new ExtendedValue(){ StringValue = email},
                                UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },

                        }
                    }
                };

                requestContainer.Add(new Request()
                {
                    AppendCells = new AppendCellsRequest()
                    {
                        Fields = '*',
                        Rows = rows,
                        SheetId = 0
                    }
                });

                var batchRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = requestContainer
                };

                await _service.Spreadsheets.BatchUpdate(batchRequest, _spreadsheetId).ExecuteAsync();

                _processQueue.Remove($"DOD-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"DOD-{_leadNumber}");
                _log.Add($"Не получилось учесть отправку КП для сделки {_leadNumber}: {e}.");
                throw;
            }
        }
    }
}