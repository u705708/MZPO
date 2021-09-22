using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("[controller]/[action]")]
    public class GSheetsController : Controller
    {
        private readonly ProcessQueue _processQueue;
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly Log _log;

        public GSheetsController(Amo amo, ProcessQueue processQueue, GSheets gSheets, Log log)
        {
            _amo = amo;
            _processQueue = processQueue;
            _gSheets = gSheets;
            _log = log;
        }

        // POST: gsheets/kp_sent
        [ActionName("KP_sent")]
        [HttpPost]
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

            _processQueue.AddTask(task, cts, $"SentKP-{leadNumber}", "mzpoeducation", "GoogleSheets");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/meeting
        [ActionName("Meeting")]
        [HttpPost]
        public IActionResult Meeting()
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

            Task task = Task.Run(() => leadProcessor.Value.CorpMeetings());

            _processQueue.AddTask(task, cts, $"CorpMeeting-{leadNumber}", "mzpoeducation", "GoogleSheets");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/dod
        [ActionName("DOD")]
        [HttpPost]
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

            _processQueue.AddTask(task, cts, $"DOD-{leadNumber}", "mzpoeducationsale", "GoogleSheets");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/nps
        [ActionName("NPS")]
        [HttpPost]
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

            _processQueue.AddTask(task, cts, $"NPS-{leadNumber}", "mzpoeducationsale", "GoogleSheets");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/reprimands
        [ActionName("Reprimands")]
        [HttpPost]
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

            _processQueue.AddTask(task, cts, $"Reprimands-{leadNumber}", "mzpoeducationsale", "GoogleSheets");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/poll
        [ActionName("Poll")]
        [HttpPost]
        public IActionResult Poll()
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

            Task task = Task.Run(() => leadProcessor.Value.Poll());

            _processQueue.AddTask(task, cts, $"Poll-{leadNumber}", "mzpoeducationsale", "GoogleSheets");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/retail
        [ActionName("Retail")]
        [HttpPost]
        public IActionResult Retail()
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

            Task task = Task.Run(() => leadProcessor.Value.Retail());

            _processQueue.AddTask(task, cts, $"Retail-{leadNumber}", "mzpoeducationsale", "GoogleSheets");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: gsheets/openlesson
        [ActionName("OpenLesson")]
        [HttpPost]
        public IActionResult OpenLesson()
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

            Task task = Task.Run(() => leadProcessor.Value.OpenLesson());

            _processQueue.AddTask(task, cts, $"OpenLesson-{leadNumber}", "mzpoeducationsale", "GoogleSheets");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

    }
}