using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.Services;
using MZPO.webinar.ru;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("api/testing")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly ProcessQueue _processQueue;
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly Log _log;
        private readonly Uber _uber;
        private readonly Cred1C _cred1C;
        private readonly RecentlyUpdatedEntityFilter _filter;
        private readonly Webinars _webinars;

        private Object locker;

        public TestingController(Amo amo, ProcessQueue processQueue, GSheets gSheets, Log log, Cred1C cred1C, RecentlyUpdatedEntityFilter filter, Uber uber, Webinars webinars)
        {
            _amo = amo;
            _processQueue = processQueue;
            _gSheets = gSheets;
            _log = log;
            _cred1C = cred1C;
            _filter = filter;
            locker = new();
            _uber = uber;
            _webinars = webinars;
        }

        private static IEnumerable<(long, long)> GetPeriods()
        {
            DateTime fromDate = new DateTime(2021, 4, 8, 0, 0, 0, DateTimeKind.Utc).AddHours(-3);
            DateTime toDate = fromDate.AddMonths(1).AddSeconds(-1);
            DateTime now = DateTime.Now;

            while (fromDate < now)
            {
                long dateFrom = ((DateTimeOffset)fromDate).ToUnixTimeSeconds();
                long dateTo = ((DateTimeOffset)toDate).ToUnixTimeSeconds();

                yield return (dateFrom, dateTo);

                fromDate = fromDate.AddMonths(1);
                toDate = toDate.AddMonths(1);
            }

            yield break;
        }

        // GET: api/testing
        [EnableCors]
        [HttpGet]
        public IActionResult Get()
        {
            //var repo = _amo.GetAccountById(29490250).GetRepo<Lead>();
            //var repo = _amo.GetAccountById(28395871).GetRepo<Lead>();
            //var repo = _amo.GetAccountById(19453687).GetRepo<Lead>();

            //return Ok(JsonConvert.SerializeObject(repo.GetTags(), Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            int i = 0;

            while (i < 12)
            {
                i++;
                UberLead lead = new() {
                    leadId = i,
                    leadName = $"Новая сделка {i}",
                    leadUri = $"https://mzpoeducationsale.amocrm.ru/leads/detail/{i}"
                };

                _uber.AddToQueue(lead);

                //Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }

            return Ok("𓅮 𓃟 𓏵 𓀠𓀡");
        }

        // POST: api/testing
        [EnableCors]
        [HttpPost]
        public IActionResult Post()
        {
            using StreamReader sr = new(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now}----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine();

            return Request.Headers["x-requested-with"] == "XMLHttpRequest" ? Ok(new { Message = "SUCCESS" }) : Ok();
        }
    }
}