using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MZPO.LeadProcessors;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("siteform/{action}")]
    public class SiteFormController : Controller
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly GSheets _gSheets;

        public SiteFormController(Amo amo, TaskList processQueue, Log log, GSheets gSheets)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
            _gSheets = gSheets;
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

            #region Adding to log
            using StreamWriter sw = new($"siteform_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} siteform/retail ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(JsonConvert.SerializeObject(col)));
            sw.WriteLine("-----");
            sw.WriteLine(WebUtility.UrlDecode(JsonConvert.SerializeObject(formRequest, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })));
            sw.WriteLine();
            #endregion

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            var leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new SiteFormRetailProcessor(_amo, _log, formRequest, _processQueue, token, _gSheets));

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

            #region Adding to log
            using StreamWriter sw = new($"siteform_requests_{DateTime.Today.ToShortDateString()}.log", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} siteform/corp ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(JsonConvert.SerializeObject(col)));
            sw.WriteLine("-----");
            sw.WriteLine(WebUtility.UrlDecode(JsonConvert.SerializeObject(formRequest, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })));
            sw.WriteLine();
            #endregion

            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;

            var leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new SiteFormCorpProcessor(_amo.GetAccountById(19453687), _log, formRequest, _processQueue, token));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, $"FormSiteCorp", "ret2corp", "LeadProcessor");                                            //Запускаем и добавляем в очередь

            return Ok();
        }
    }
}