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
    public class CorpKpSentProcessor : ILeadProcessor
    {
        private readonly TaskList _processQueue;
        private readonly CancellationToken _token;
        private readonly Log _log;
        private readonly IAmoRepo<Lead> _leadRepo;
        private readonly IAmoRepo<Company> _compRepo;
        private readonly int _leadNumber;
        private readonly SheetsService _service;
        private readonly string _spreadsheetId;

        public CorpKpSentProcessor(int leadnumber, Amo amo, GSheets gSheets, TaskList processQueue, Log log, CancellationToken token)
        {
            _processQueue = processQueue;
            _token = token;
            _log = log;
            _leadRepo = amo.GetAccountById(19453687).GetRepo<Lead>();
            _compRepo = amo.GetAccountById(19453687).GetRepo<Company>();
            _leadNumber = leadnumber;
            _service = gSheets.GetService();
            _spreadsheetId = "1xuxd7RfHTTCtalfbXfyJbx2BuNb5RflxwqgtlBamQFM";
        }

        private readonly List<(int, string)> managers = new()
        {
            (2375116, "Киреева Светлана"),
            (2375131, "Алферова Лилия"),
            (6904255, "Виктория Корчагина"),
            (6909061, "Оксана Строганова"),
            (2884132, "Ирина Сорокина"),
            (6028753, "Алена Федосова"),
            (6630727, "Елена Зубатых"),
            (6697522, "Наталья Филатова"),
            (3770773, "Шталева Лидия"),
            (6200629, "Харшиладзе Леван"),
            (6346882, "Мусихина Юлия")
        };

        public async Task Run()
        {
            if (_token.IsCancellationRequested)
            {
                _processQueue.Remove($"SentKP-{_leadNumber}");
                return;
            }
            try
            {
                Lead lead;
                try { lead = _leadRepo.GetById(_leadNumber); }
                catch { lead = null; }

                if (lead is null ||
                    lead._embedded is null ||
                    lead._embedded.companies is null)
                    return;

                var leadName = lead.name;
                var leadId = lead.id.ToString();
                var date = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.AddHours(-2).ToShortTimeString()}";

                if (!managers.Any(x => x.Item1 == lead.responsible_user_id))
                    throw new Exception($"Unknown manager: {lead.responsible_user_id}");

                string companyName;
                try { companyName = _compRepo.GetById(lead._embedded.companies.First().id).name; }
                catch { companyName = ""; }

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
                                UserEnteredValue = new ExtendedValue(){ StringValue = leadName},
                                UserEnteredFormat = new CellFormat(){ NumberFormat = new NumberFormat() { Type = "TEXT" } } },
                            new CellData(){
                                UserEnteredValue = new ExtendedValue(){ StringValue = companyName},
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
                        SheetId = lead.responsible_user_id
                    }
                });

                var batchRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = requestContainer
                };

                await _service.Spreadsheets.BatchUpdate(batchRequest, _spreadsheetId).ExecuteAsync();

                _processQueue.Remove($"SentKP-{_leadNumber}");
            }
            catch (Exception e)
            {
                _processQueue.Remove($"SentKP-{_leadNumber}");
                _log.Add($"Не получилось учесть отправку КП для сделки {_leadNumber}: {e}.");
                throw;
            }
        }
    }
}