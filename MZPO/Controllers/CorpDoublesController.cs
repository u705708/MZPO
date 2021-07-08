using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("wh/corp/{action}")]
    public class CorpDoublesController : Controller
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Cred1C _cred1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public CorpDoublesController(Amo amo, TaskList processQueue, Log log, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
            _cred1C = cred1C;
            _filter = filter;
        }

        // POST: wh/corp/checkdouble
        [HttpPost]
        [ActionName("CheckDouble")]
        public IActionResult CheckDouble()
        {
            var col = Request.Form;

            AmoAccount acc;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            if (!col.ContainsKey("contacts[update][0][id]")) return BadRequest("Unexpected request.");
            if (!Int32.TryParse(col["contacts[update][0][id]"], out int companyNumber)) return BadRequest("Incorrect lead number.");

            if (!_filter.CheckEntityIsValid(companyNumber))
                return Ok();

            CancellationTokenSource cts = new();

            Lazy<ILeadProcessor> leadProcessor = new(() =>
                   new SmilarcompaniesCheckProcessor(companyNumber, acc, _processQueue, _log, cts.Token, _filter));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, companyNumber.ToString(), acc.name, "DoublesCheck");

            return Ok();
        }
    }
}