using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.Processors;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("api/testing")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;

        public TestingController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(28395871);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "";
        }

        // GET: api/testing
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

            return Ok(list.Count);
        }
    }
}