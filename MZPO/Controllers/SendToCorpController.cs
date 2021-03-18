using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("wh/sendtocorp")]
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

        // POST wh/sendtocorp
        [HttpPost]
        public IActionResult Post()
        {
            var col = Request.Form;
            int leadNumber = 0;

            Lazy<ILeadProcessor> leadProcessor;
            Task task;

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (col.ContainsKey("leads[status][0][id]"))                                                                                        //Смена статусв
            {
                if (!Int32.TryParse(col["leads[status][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }

            if (leadNumber == 0) return BadRequest("Incorrect lead number");

            leadProcessor = new Lazy<ILeadProcessor>(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new SendToCorpProcessor(_amo, _log, _processQueue, leadNumber, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"ret2corp-{leadNumber}", "ret2corp", "LeadProcessor");                                            //Запускаем и добавляем в очередь
            return Ok();
        }
    }
}