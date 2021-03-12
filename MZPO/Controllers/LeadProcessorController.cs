using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("wh/leadprocessor")]
    [ApiController]
    public class LeadProcessorController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;

        public LeadProcessorController(Amo amo, TaskList processQueue, Log log)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
        }

        // GET wh/leadprocessor/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)                                                                                                      //Передаём вручную сделку в процессор (в дальнейшем заменить на POST)
        {
            if (!Int32.TryParse(id, out int leadNumber)) return BadRequest("Incorrect lead number.");

            Task task;
            var acc = _amo.GetAccountById(28395871);
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<ILeadProcessor> leadProcessor = new Lazy<ILeadProcessor>(() =>                                                                 //Создаём экземпляр процессора сделки
                               new InitialLeadProcessor(leadNumber, acc, _processQueue, _log, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, leadNumber.ToString(), acc.name, "LeadProcessor");                                                 //Запускаем и добавляем в очередь
            return Ok();
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

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            if(!Int32.TryParse(col["account[id]"], out int accNumber)) return BadRequest("Incorrect account number.");
            
            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { _log.Add(e.Message); return Ok(); }

            #region Parsing hook
            if (col.ContainsKey("leads[add][0][id]"))                                                                                           //Создана новая сделка
            {
                if(!Int32.TryParse(col["leads[add][0][id]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }
            else if (col["unsorted[delete][0][action]"] == "accept")                                                                            //Сделка принята из Неразобранного
            {
                if(!Int32.TryParse(col["unsorted[delete][0][accept_result][leads][0]"], out leadNumber)) return BadRequest("Incorrect lead number.");
            }
            else if (col.ContainsKey("unsorted[add][0][source_data][service]") &&                                                               //Сделка создана в Неразобранном
                    (col["unsorted[add][0][source_data][service]"] != "com.wazzup24.wz"))                                                       //Не из Wazzup
            {
                leadProcessor = new Lazy<ILeadProcessor>(() =>
                    new UnsortedProcessor(col["unsorted[add][0][uid]"], acc, _processQueue, _log, token));
                
                task = Task.Run(() => leadProcessor.Value.Run());
                _processQueue.AddTask(task, cts, col["unsorted[add][0][uid]"], acc.name, "UnsortedProcessor");
                return Ok();
            }
            else return Ok();
            #endregion

            leadProcessor = new Lazy<ILeadProcessor>( () =>                                                                                     //Создаём экземпляр процессора сделки
                                new InitialLeadProcessor(leadNumber, acc, _processQueue, _log, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, leadNumber.ToString(), acc.name, "LeadProcessor");                                                 //Запускаем и добавляем в очередь
            return Ok();
        }
    }
}