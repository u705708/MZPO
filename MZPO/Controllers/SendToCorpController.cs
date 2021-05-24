using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("ret2corp/{action}")]
    public class SendToCorpController : Controller
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;

        public SendToCorpController(Amo amo, TaskList processQueue, Log log)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
        }

        // POST ret2corp/send
        [HttpPost]
        public IActionResult Send()
        {
            var col = Request.Form;
            int leadNumber = 0;

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            Lazy<SendToCorpProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToCorpProcessor(_amo, _log, _processQueue, leadNumber, token));

            Task task = Task.Run(() => leadProcessor.Value.Send());
            _processQueue.AddTask(task, cts, $"ret2corp-{leadNumber}", "ret2corp", "SyncProcessor");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST ret2corp/success
        [HttpPost]
        public IActionResult Success()
        {
            var col = Request.Form;
            int leadNumber = 0;

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            Lazy<SendToCorpProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToCorpProcessor(_amo, _log, _processQueue, leadNumber, token));

            Task task = Task.Run(() => leadProcessor.Value.Success());
            _processQueue.AddTask(task, cts, $"corp2ret-{leadNumber}", "ret2corp", "SyncProcessor");                                            //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST ret2corp/fail
        [HttpPost]
        public IActionResult Fail()
        {
            var col = Request.Form;
            int leadNumber = 0;

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            Lazy<SendToCorpProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToCorpProcessor(_amo, _log, _processQueue, leadNumber, token));

            Task task = Task.Run(() => leadProcessor.Value.Fail());
            _processQueue.AddTask(task, cts, $"corp2ret-{leadNumber}", "ret2corp", "SyncProcessor");                                            //Запускаем и добавляем в очередь
            return Ok();
        }
    }
}