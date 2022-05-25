using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.LeadProcessors;
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
            DateTime fromDate = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(-3);
            DateTime toDate = fromDate.AddDays(1).AddSeconds(-1);
            DateTime now = DateTime.Now;

            while (fromDate < now)
            {
                long dateFrom = ((DateTimeOffset)fromDate).ToUnixTimeSeconds();
                long dateTo = ((DateTimeOffset)toDate).ToUnixTimeSeconds();

                yield return (dateFrom, dateTo);

                fromDate = fromDate.AddDays(1);
                toDate = toDate.AddDays(1);
            }

            yield break;
        }

        public class Import
        {
            public string resp { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
            public string course { get; set; }
            public string comment1 { get; set; }
            public string title { get; set; }
            public string comment2 { get; set; }
        }

        Dictionary<string, int> managers = new Dictionary<string, int>()
        {
            {"Алёна Федосова", 6028753},
            {"Алферова Лилия", 2375131},
            {"Ирина Сорокина", 2884132},
            {"Киреева Светлана", 2375116},
            {"Ленара Бекташева", 7523557},
            {"Лоскутова Анастасия", 7532620},
            {"Саланович Эллада", 7358626},
            {"Cистемный администратор", 2375146},
        };



        // GET: api/testing
        [EnableCors]
        [HttpGet]
        public IActionResult Get()
        {
            //var repo = _amo.GetAccountById(29490250).GetRepo<Lead>();
            var repo = _amo.GetAccountById(28395871).GetRepo<Lead>();
            //var _leadRepo = _amo.GetAccountById(19453687).GetRepo<Lead>();
            //var _contRepo = _amo.GetAccountById(19453687).GetRepo<Contact>();

            //return Ok(JsonConvert.SerializeObject(repo.GetTags(), Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            //return Ok(new MZPOEvent("Встреча \"Международного клуба выпускников\"").GetPropertiesAsync().GetAwaiter().GetResult());

            //IEnumerable<Lead> leads = repo.GetByCriteria($"filter[pipeline_id][0]=3198184&filter[created_at][from]={0}&filter[created_at][to]={0}");

            //foreach (var p in GetPeriods())
            //{
            //    leads = leads.Concat(repo.GetByCriteria($"filter[pipeline_id][0]=3198184&filter[closed_at][from]={p.Item1}&filter[closed_at][to]={p.Item2}"));
            //}

            //int lp = 0;
            //int lc = 0;
            //int le = 0;

            //Parallel.ForEach(
            //    leads,
            //    new ParallelOptions { MaxDegreeOfParallelism = 6 },
            //    l =>
            //    {
            //        lock (locker)
            //            lp++;

            //        if (!l.HasCF(357005))
            //            return;

            //        try
            //        {
            //            Lead newLead = new() { id = l.id };

            //            string course = l.GetCFStringValue(357005).ToUpper().Trim();
            //            newLead.AddNewCF(357005, course.Length > 255 ? course.Substring(0, 255) : course);

            //            repo.Save(newLead);

            //            lock (locker)
            //                lc++;
            //        }
            //        catch
            //        {
            //            lock (locker)
            //                le++;
            //        }
            //    });

            //return Ok($"processed: {lp}, changed: {lc}, exception: {le}");

            //return Ok(repo.GetTags().ToList());

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

            //using FileStream stream = new("import.json", FileMode.Open, FileAccess.Read);
            //using StreamReader sr = new(stream);
            //List<Import> imports = JsonConvert.DeserializeObject<List<Import>>(sr.ReadToEnd());

            //List<Import> unprocessed = new();
            //i = 0;


            //foreach(var imp in imports)
            //{
            //    try
            //    {
            //        i++;
            //        Lead lead = new()
            //        {
            //            name = imp.name,
            //            responsible_user_id = managers[imp.resp],
            //            pipeline_id = 5312269,
            //            status_id = 47317618,
            //            _embedded = new()
            //        };

            //        if (!string.IsNullOrEmpty(imp.course))
            //            lead.AddNewCF(118509, imp.course);

            //        List<Contact> similarContacts = new();

            //        Contact contact = new()
            //        {
            //            name = imp.name,
            //            responsible_user_id = managers[imp.resp],
            //            custom_fields_values = new()
            //        };

            //        if (!string.IsNullOrEmpty(imp.name))
            //            similarContacts.AddRange(_contRepo.GetByCriteria($"query={imp.name}"));

            //        if (similarContacts.Any(x => x.name == imp.name))
            //            contact.id = similarContacts.OrderBy(x => x.created_at).First(x => x.name == imp.name).id;

            //        if (!string.IsNullOrEmpty(imp.email))
            //            contact.AddNewCF(33577, imp.email);

            //        if (!string.IsNullOrEmpty(imp.phone))
            //            contact.AddNewCF(33575, imp.phone);

            //        lead._embedded.contacts = new() { contact };

            //        var createdId = _leadRepo.AddNewComplex(lead).First();

            //        if (!string.IsNullOrEmpty(imp.comment1))
            //            _leadRepo.AddNotes(createdId, imp.comment1);

            //        if (!string.IsNullOrEmpty(imp.comment2))
            //            _leadRepo.AddNotes(createdId, imp.comment2);
            //    }
            //    catch
            //    {
            //        lock (locker) { unprocessed.Add(imp); }
            //    }
            //};
            
            //return Ok(unprocessed);
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