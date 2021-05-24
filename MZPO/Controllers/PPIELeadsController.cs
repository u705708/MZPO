using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("wh/ppie")]
    [ApiController]
    public class PPIELeadsController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;

        public PPIELeadsController(Amo amo, TaskList processQueue, Log log, LeadsSorter sorter)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
        }

        // POST wh/leadprocessor
        [HttpPost]
        public IActionResult Post()
        {
            var col = Request.Form;
            int leadNumber = 0;
            AmoAccount acc;

            Lazy<ILeadProcessor> leadProcessor;
            Task task;

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            if (!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");

            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            #region Parsing hook
            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if (!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }
            else if (col["unsorted[delete][0][action]"] == "accept")                                                                            //Сделка принята из Неразобранного
            {
                if (!Int32.TryParse(col["unsorted[delete][0][accept_result][leads][0]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }
            else if (col.ContainsKey("unsorted[add][0][source_data][service]") &&                                                               //Сделка создана в Неразобранном
                     col["unsorted[add][0][source_data][service]"] != "com.wazzup24.wz" &&                                                      //Не из Wazzup
                     col["unsorted[add][0][source_data][service]"] != "com.wazzup24.insta")                                                     //Не из Wazzup
            {
                leadProcessor = new Lazy<ILeadProcessor>(() =>
                    new UnsortedProcessor(col["unsorted[add][0][uid]"], acc, _processQueue, _log, token));

                task = Task.Run(() => leadProcessor.Value.Run());
                _processQueue.AddTask(task, cts, col["unsorted[add][0][uid]"], acc.name, "UnsortedProcessor");
                return Ok();
            }
            else return Ok();
            #endregion

            leadProcessor = new Lazy<ILeadProcessor>(() =>                                                                                     //Создаём экземпляр процессора сделки
                               new PPIELeadsProcessor(leadNumber, acc, _processQueue, _log, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, leadNumber.ToString(), acc.name, "LeadProcessor");                                                 //Запускаем и добавляем в очередь
            return Ok();
        }
    }
}