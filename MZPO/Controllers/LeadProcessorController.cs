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

        public LeadProcessorController(Amo amo, TaskList processQueue)
        {
            _amo = amo;
            _processQueue = processQueue;
        }

        // GET wh/leadprocessor/5
        [HttpGet("{id}")]
        public ActionResult Get(string id)                                                                              //Передаём вручную сделку в процессор (в дальнейшем заменить на POST)
        {
            if (!Int32.TryParse(id, out int leadNumber)) return BadRequest("Incorrect lead number");

            var acc = _amo.GetAccountById(28395871);
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IProcessor> initialLeadProcessor = new Lazy<IProcessor>(() =>                      //Создаём экземпляр процессора сделки
                               new InitialLeadProcessor(leadNumber, acc, _processQueue, token));

            Task task = Task.Run(() => initialLeadProcessor.Value.Run());                                               //Запускаем его
            _processQueue.AddTask(task, cts, id, acc.name, "LeadProcessor");                                                //И добавляем в очередь
            return Ok();
        }

        // POST wh/leadprocessor
        [HttpPost]
        public ActionResult Post()
        {
            var col = Request.Form;
            int leadNumber = 0;
            Task task;
            AmoAccount acc;

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Int32.TryParse(col["account[id]"], out int accNumber);
            
            try { acc = _amo.GetAccountById(accNumber); }
            catch (Exception e) { Log.Add(e.Message); return Ok(); }

            #region Parsing hook
            if (col.ContainsKey("leads[add][0][id]"))                                                                   //Создана новая сделка
            {
                Int32.TryParse(col["leads[add][0][id]"], out leadNumber);
            }
            else if (col["unsorted[delete][0][action]"] == "accept")                                                    //Сделка принята из Неразобранного
            {
                Int32.TryParse(col["unsorted[delete][0][accept_result][leads][0]"], out leadNumber);
            }
            else if (col.ContainsKey("unsorted[add][0][source_data][service]")                                          //Сделка создана в Неразобранном
                    && (col["unsorted[add][0][source_data][service]"] != "com.wazzup24.wz"))                            //Не из Wazzup
            {
                Lazy<UnsortedProcessor> unsortedProcessor = new Lazy<UnsortedProcessor>(() =>
                                   new UnsortedProcessor(col["unsorted[add][0][uid]"], acc, _processQueue, token));
                task = Task.Run(() => unsortedProcessor.Value.Run());
                _processQueue.AddTask(task, cts, col["unsorted[add][0][uid]"], acc.name, "UnsortedProcessor");
                return Ok();
            }
            else return Ok();
            #endregion

            Lazy<IProcessor> initialLeadProcessor = new Lazy<IProcessor>( () =>                     //Создаём экземпляр процессора сделки
                                new InitialLeadProcessor(leadNumber, acc, _processQueue, token));
            
            task = Task.Run(() => initialLeadProcessor.Value.Run());                                                    //Запускаем его
            _processQueue.AddTask(task, cts, leadNumber.ToString(), acc.name, "LeadProcessor");                             //И добавляем в очередь
            return Ok();
        }
    }
}