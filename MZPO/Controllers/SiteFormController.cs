using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("siteform/{action}")]
    public class SiteFormController : Controller
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly Log _log;

        public SiteFormController(Amo amo, TaskList processQueue, GSheets gSheets, Log log)
        {
            _amo = amo;
            _processQueue = processQueue;
            _gSheets = gSheets;
            _log = log;
        }

        // POST: siteform/retail
        [EnableCors]
        [HttpPost]
        public IActionResult Retail()
        {
            if (Request.ContentType != "application/x-www-form-urlencoded") return BadRequest();

            var col = Request.Form;

            FormRequest formRequest = new();

            #region Parsing post
            foreach (var p in formRequest.GetType().GetProperties())
                if (col.ContainsKey(p.Name))
                    if(col.TryGetValue(p.Name, out var value))
                        p.SetValue(formRequest, (string)value);
            #endregion

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            var leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new SiteFormRetailProcessor(_amo.GetAccountById(28395871), _log, formRequest, _processQueue, token));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"FormSiteRet", "ret2corp", "LeadProcessor");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: siteform/corp
        [EnableCors]
        [HttpPost]
        public IActionResult Corp()
        {
            if (Request.ContentType != "application/x-www-form-urlencoded") return BadRequest();

            var col = Request.Form;

            FormRequest formRequest = new();

            #region Parsing post
            foreach (var p in formRequest.GetType().GetProperties())
                if (col.ContainsKey(p.Name))
                    if (col.TryGetValue(p.Name, out var value))
                        p.SetValue(formRequest, (string)value);
            #endregion

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            var leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new SiteFormCorpProcessor(_amo.GetAccountById(19453687), _log, formRequest, _processQueue, token));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"FormSiteCorp", "ret2corp", "LeadProcessor");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: siteform/kp_sent
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

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Lazy<ILeadProcessor> leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new CorpKpSentProcessor(leadNumber, _amo, _gSheets, _processQueue, _log, token));

            Task task = Task.Run(() => leadProcessor.Value.Run());

            _processQueue.AddTask(task, cts, $"SentKP-{leadNumber}", "ret2corp", "LeadProcessor");                                            //Запускаем и добавляем в очередь

            return Ok();
        }
    }
}