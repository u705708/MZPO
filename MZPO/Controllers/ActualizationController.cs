using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;

namespace MZPO.Controllers
{
    [Route("preparereports/act")]
    [ApiController]
    public class ActualizationController : ControllerBase
    {
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;

        public ActualizationController(Amo amo, GSheets gSheets)
        {
            _acc = amo.GetAccountById(28395871);
            _gSheets = gSheets;
            sheetId = "1oYMRm8yrkjLgehx77Ez3K4-YothSq-9_ClZ5-HeNSKA";
        }

        // GET: preparereports/act
        [HttpGet]
        public ActionResult Get()
        {
            var leadRepo = _acc.GetRepo<Lead>();

            var d2 = DateTime.Today.AddSeconds(-1);
            var d1 = DateTime.Today.AddDays(-1);
            var du2 = (int)((DateTimeOffset)d2).ToUnixTimeSeconds();
            var du1 = (int)((DateTimeOffset)d1).ToUnixTimeSeconds();

            var criteria = $"filter[created_at][from]={du1}&filter[created_at][to]={du2}&filter[created_by][]=6158035&filter[entity][]=lead&filter[type][]=lead_status_changed&filter[value_before][leads_statuses][0][pipeline_id]=3558922&filter[value_before][leads_statuses][0][status_id]=35002129";
            var list = new List<Event>();
            var result = leadRepo.GetEventsByCriteria(criteria);

            if (result is not null)
                list.AddRange(result);

            var rows = new List<RowData>
            {
                new RowData()
                {
                    Values = new List<CellData>(){
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ StringValue = d1.ToShortDateString()} },
                         new CellData(){
                             UserEnteredValue = new ExtendedValue(){ NumberValue = list.Count} }
                    }
                }
            };

            List<Request> requestContainer = new List<Request>(){ new Request()
            {
                AppendCells = new AppendCellsRequest()
                {
                    Fields = '*',
                    Rows = rows,
                    SheetId = 0
                }
            } };

            var batchRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = requestContainer
            };

            _gSheets.GetService().Spreadsheets.BatchUpdate(batchRequest, sheetId).ExecuteAsync();

            return Ok();
        }
    }
}