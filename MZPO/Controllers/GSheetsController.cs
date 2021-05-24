using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("gsheets/{action}")]
    public class GSheetsController : Controller
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly Log _log;

        public GSheetsController(Amo amo, TaskList processQueue, GSheets gSheets, Log log)
        {
            _amo = amo;
            _processQueue = processQueue;
            _gSheets = gSheets;
            _log = log;
        }

        // POST: gsheets/kp_sent
        public IActionResult KP_sent()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[status][0][id]"))
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[add][0][id]"))
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return Ok();

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            Lazy<GSheetsProcessor> leadProcessor = new(() =>
                   new GSheetsProcessor(leadNumber, _amo, _gSheets, _processQueue, _log, token));

            Task task = Task.Run(() => leadProcessor.Value.CorpKP());

            _processQueue.AddTask(task, cts, $"SentKP-{leadNumber}", "KP_sent", "LeadProcessor");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/dod
        public IActionResult DOD()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[status][0][id]"))
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[add][0][id]"))
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return Ok();

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            Lazy<GSheetsProcessor> leadProcessor = new(() =>
                   new GSheetsProcessor(leadNumber, _amo, _gSheets, _processQueue, _log, token));

            Task task = Task.Run(() => leadProcessor.Value.DOD());

            _processQueue.AddTask(task, cts, $"DOD-{leadNumber}", "DOD", "LeadProcessor");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/nps
        public IActionResult NPS()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[status][0][id]"))
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[add][0][id]"))
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return Ok();

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            Lazy<GSheetsProcessor> leadProcessor = new(() =>
                   new GSheetsProcessor(leadNumber, _amo, _gSheets, _processQueue, _log, token));

            Task task = Task.Run(() => leadProcessor.Value.NPS());

            _processQueue.AddTask(task, cts, $"NPS-{leadNumber}", "NPS", "LeadProcessor");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/reprimands
        public IActionResult Reprimands()
        {
            var col = Request.Form;
            int leadNumber = 0;

            if (col.ContainsKey("leads[status][0][id]"))
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[add][0][id]"))
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return Ok();

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            Lazy<GSheetsProcessor> leadProcessor = new(() =>
                   new GSheetsProcessor(leadNumber, _amo, _gSheets, _processQueue, _log, token));

            Task task = Task.Run(() => leadProcessor.Value.Reprimands());

            _processQueue.AddTask(task, cts, $"Reprimands-{leadNumber}", "Reprimands", "LeadProcessor");                                            //Запускаем и добавляем в очередь

            return Ok();
        }
    }
}