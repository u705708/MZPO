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
    [Route("[controller]/[action]")]
    public class SiteFormController : Controller
    {
        private readonly ProcessQueue _processQueue;
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly GSheets _gSheets;
        private readonly string _path;

        public SiteFormController(Amo amo, ProcessQueue processQueue, Log log, GSheets gSheets)
        {
            _amo = amo;
            _processQueue = processQueue;
            _log = log;
            _gSheets = gSheets;
            _path = $@"logs\siteform\{DateTime.Today.Year}-{DateTime.Today.Month}-{DateTime.Today.Day}.log";
        }

        // POST: siteform/retail
        [EnableCors]
        [ActionName("Retail")]
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
            using StreamWriter sw = new(_path, true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} siteform/retail ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(JsonConvert.SerializeObject(col)));
            sw.WriteLine("-----");
            sw.WriteLine(WebUtility.UrlDecode(JsonConvert.SerializeObject(formRequest, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })));
            sw.WriteLine();
            #endregion

            CancellationTokenSource cts = new();

            string taskName = $"FormSiteRet-{DateTime.Now.ToLongTimeString()}";

            var leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new SiteFormRetailProcessor(_amo, _log, formRequest, _processQueue, cts.Token, _gSheets, taskName));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, taskName, "mzpoeducationsale", "SiteForm");                                            //Запускаем и добавляем в очередь

            return Ok();
        }

        // POST: siteform/corp
        [EnableCors]
        [ActionName("Corp")]
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
            using StreamWriter sw = new(_path, true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now} siteform/corp ----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(JsonConvert.SerializeObject(col)));
            sw.WriteLine("-----");
            sw.WriteLine(WebUtility.UrlDecode(JsonConvert.SerializeObject(formRequest, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })));
            sw.WriteLine();
            #endregion

            CancellationTokenSource cts = new();

            string taskName = $"FormSiteCorp-{DateTime.Now.ToLongTimeString()}";

            var leadProcessor = new Lazy<ILeadProcessor>(() =>
                   new SiteFormCorpProcessor(_amo, _log, formRequest, _processQueue, cts.Token, _gSheets, taskName));

            Task task = Task.Run(() => leadProcessor.Value.Run());
            _processQueue.AddTask(task, cts, taskName, "mzpoeducation", "SiteForm");                                            //Запускаем и добавляем в очередь

            return Ok();
        }
    }
}