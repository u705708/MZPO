using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("wh")]
    [Route("[controller]")]
    public class WebhookController : Controller
    {
        private readonly ProcessQueue _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly RecentlyUpdatedEntityFilter _filter;
        private readonly GSheets _gSheets;

        public WebhookController(Amo amo, ProcessQueue processQueue, Log log, RecentlyUpdatedEntityFilter filter, GSheets gSheets)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
            _filter = filter;
            _gSheets = gSheets;
        }

        // GET wh/leadprocessor/5
        [Route("[action]/{id}")]
        [ActionName("LeadProcessor")]
        [HttpGet("{id}")]
        public IActionResult LeadProcessor(string id)                                                                                                      //Передаём вручную сделку в процессор (в дальнейшем заменить на POST)
        {
            if (!Int32.TryParse(id, out int leadNumber)) return BadRequest("Incorrect lead number.");

            Task task;
            var acc = _amo.GetAccountById(28395871);
            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;
            Lazy<ILeadProcessor> leadProcessor = new(() =>                                                                                      //Создаём экземпляр процессора сделки
                               new InitialLeadProcessor(leadNumber, acc, _amo, _gSheets, _processQueue, _log, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, leadNumber.ToString(), acc.name, "LeadProcessor");                                                 //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/leadprocessor
        [Route("[action]")]
        [ActionName("LeadProcessor")]
        [HttpPost]
        public IActionResult LeadProcessor()
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
                               new InitialLeadProcessor(leadNumber, acc, _amo, _gSheets, _processQueue, _log, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, leadNumber.ToString(), acc.name, "LeadProcessor");                                                 //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/ppie
        [Route("[action]")]
        [ActionName("PPIE")]
        [HttpPost]
        public IActionResult PPIE()
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
            else return Ok();
            #endregion

            leadProcessor = new Lazy<ILeadProcessor>(() =>                                                                                     //Создаём экземпляр процессора сделки
                               new PPIELeadsProcessor(leadNumber, acc, _processQueue, _log, token));

            task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, leadNumber.ToString(), acc.name, "LeadProcessor");                                                 //Запускаем и добавляем в очередь
            return Ok();
        }

        // POST wh/ret/setcourse
        [Route("ret/[action]")]
        [ActionName("SetCourse")]
        [HttpPost]
        public IActionResult SetCourse()
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

        // POST: wh/corp/checkdouble
        [Route("corp/[action]")]
        [ActionName("CheckDouble")]
        [HttpPost]
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

        // POST ret2corp/send
        [Route("ret2corp/[action]")]
        [ActionName("Send")]
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
        [Route("ret2corp/[action]")]
        [ActionName("Success")]
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
        [Route("ret2corp/[action]")]
        [ActionName("Fail")]
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