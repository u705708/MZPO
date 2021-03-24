using Google.Apis.Sheets.v4;
using Integration1C;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.LeadProcessors;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("api/testing")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly GSheets _gSheets;
        private readonly Log _log;
        private readonly Cred1C _cred1C;

        public TestingController(Amo amo, TaskList processQueue, GSheets gSheets, Log log, Cred1C cred1C)
        {
            _amo = amo;
            _processQueue = processQueue;
            _gSheets = gSheets;
            _log = log;
            _cred1C = cred1C;
        }

        public class Entry
        {
            public int payment_received;
            public int payment_amount;
            public string receipt_number;
            public string manager;
            public string client_name;
        }

        class ContactsComparer : IEqualityComparer<Contact>
        {
            public bool Equals(Contact x, Contact y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (x is null || y is null)
                    return false;

                return x.id == y.id;
            }

            public int GetHashCode(Contact c)
            {
                if (c is null) return 0;

                int hashProductCode = (int)c.id;

                return hashProductCode;
            }
        }

        // GET: api/testing
        [EnableCors]
        [HttpGet]
        public IActionResult Get()
        {
            //var contRepo = _amo.GetAccountById(19453687).GetRepo<Contact>();
            //var contRepo = _amo.GetAccountById(28395871).GetRepo<Contact>();

            //var leadRepo = _amo.GetAccountById(28395871).GetRepo<Lead>();

            //return Ok(leadRepo.GetById(23889501));

            //List<int> ids = new()
            //{
            //    23889501,
            //    23889499,
            //    23889469,
            //    23889475,
            //    23889419,
            //    23889383,
            //    23889343,
            //};

            //List<Lead> leads = new();
            //Parallel.ForEach(ids, i =>  leads.Add(leadRepo.GetById(i)) );

            //return Ok(leads);

            return Ok("𓅮 𓃟 𓏵 𓀠𓀡");

            #region CorpParse
            //var _spreadsheetId = "1NuP1qpKDuWlQAje0mIA4i73KgfTH6TGi5iLvzMY46pU";
            //var range = "Сводные!A:F";
            //var _service = _gSheets.GetService();
            //var request = _service.Spreadsheets.Values.Get(_spreadsheetId, range);
            //request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;
            //var values = request.Execute().Values;

            //List<Entry> data = new();

            //if (values is not null)
            //    foreach (var row in values)
            //    {
            //        if ((string)row[0] == "дата") continue;
            //        DateTime A = Convert.ToDateTime(row[0]);
            //        int B = (int)Convert.ToDouble(row[1]);
            //        string C = Convert.ToString(row[2]);
            //        string D = Convert.ToString(row[3]);
            //        string E = Convert.ToString(row[4]);
            //        string F = Convert.ToString(row[5]);

            //        data.Add(new()
            //        {
            //            payment_received = (int)((DateTimeOffset)A.AddHours(2)).ToUnixTimeSeconds(),
            //            payment_amount = B,
            //            receipt_number = E,
            //            manager = F,
            //            client_name = C //Распарсить
            //        });
            //    }

            //return Ok(JsonConvert.SerializeObject(data, Formatting.Indented));
            #endregion

            #region ActualizatonResponsibleCheck
            //var leadRepo = _amo.GetAccountById(28395871).GetRepo<Lead>();

            //List<(int, string)> managers = new List<(int, string)>
            //{
            //    (2375107, "Кристина Гребенникова"),
            //    (2375143, "Екатерина Белоусова"),
            //    (2976226, "Вера Гладкова"),
            //    (3835801, "Наталья Кубышина"),
            //    (6158035, "Анастасия Матюк"),
            //    (2375152, "Карен Оганисян"),
            //    (3813670, "Федорова Александра"),
            //    (6102562, "Валерия Лукьянова"),
            //    (6410290, "Вероника Бармина"),
            //    (6699043, "Татьяна Ганоу"),
            //    (6729241, "Серик Айбасов")
            //};

            //var d2 = new DateTime(2021, 2, 3).AddHours(2).AddSeconds(-1);
            //var d1 = new DateTime(2021, 2, 2).AddHours(2);
            //var du2 = (int)((DateTimeOffset)d2).ToUnixTimeSeconds();
            //var du1 = (int)((DateTimeOffset)d1).ToUnixTimeSeconds();

            //var criteria = $"filter[created_at][from]={du1}&filter[created_at][to]={du2}&filter[entity][]=lead&filter[type][]=entity_responsible_changed&filter[value_after][responsible_user_id]=6158035";
            //var eventsList = new List<Event>();
            //var result = leadRepo.GetEventsByCriteria(criteria);

            //if (result is not null)
            //    eventsList.AddRange(result);

            //var leadIdList = eventsList.Select(x => (x._embedded.entity.id, x.value_before[0].responsible_user.id));

            //List<Lead> newLeads = new();

            //Parallel.ForEach(leadIdList, l =>
            //{
            //    int responsibleId = l.Item2;
            //    var lead = leadRepo.GetById(l.Item1);
            //    if (lead.pipeline_id != 3558922) return;
            //    if (lead.responsible_user_id == responsibleId) return;
            //    if (!managers.Any(x => x.Item1 == responsibleId)) responsibleId = 2375107;

            //    Lead newLead = new()
            //    {
            //        id = lead.id,
            //        responsible_user_id = responsibleId
            //    };
            //    newLeads.Add(newLead);
            //});

            //int i = 0;
            //List<Lead> leadsToSave = new();

            //foreach (var l in newLeads)
            //{
            //    leadsToSave.Add(l);
            //    i++;
            //    if (i % 10 != 0) continue;
            //    leadRepo.Save(leadsToSave);
            //    leadsToSave = new();
            //}

            //return Ok();
            #endregion

            #region DupeCheck
            //var contRepo = _amo.GetAccountById(28395871).GetRepo<Contact>();

            ////var dates = (1559336400, 1567285199);
            ////var dates = (1567285200, 1569877199);
            ////var dates = (1569877200, 1572555599); //01-31.10.2019
            ////var dates = (1572555600, 1575147599); //01-30.11.2019 ---
            ////var dates = (1575147600, 1577825999); //01-31.12.2019
            ////var dates = (1577826000, 1580504399);
            ////var dates = (1580504400, 1583009999);
            ////var dates = (1583010000, 1585688399);
            ////var dates = (1585688400, 1588280399);
            ////var dates = (1588280400, 1590958799);//++
            //var dates = (1590958800, 1593550799);
            ////var dates = (1593550800, 1596229199);
            ////var dates = (1596229200, 1598907599);
            ////var dates = (1598907600, 1601499599);
            ////var dates = (1601499600, 1604177999);
            ////var dates = (1604178000, 1606769999);
            ////var dates = (1606770000, 1609448399);
            ////var dates = (1609448400, 1612126799);
            ////var dates = (1612126800, 1614545999);
            ////var dates = (1614546000, 1617224399);

            //var criteria = $"filter[created_at][from]={dates.Item1}&filter[created_at][to]={dates.Item2}";

            //var contacts = contRepo.GetByCriteria(criteria);

            //List<(int, string)> doubleContacts = new();

            //Parallel.ForEach(
            //    contacts,
            //    new ParallelOptions { MaxDegreeOfParallelism = 5 }, 
            //    c =>
            //{
            //    List<Contact> contactsWithSimilarPhone = new();
            //    List<Contact> contactsWithSimilarMail = new();

            //    if (c.custom_fields_values is null) return;

            //    if (c.custom_fields_values.Any(x => x.field_id == 264911))
            //        foreach (var v in c.custom_fields_values.First(x => x.field_id == 264911).values)
            //            if ((string)v.value != "" &&
            //                (string)v.value != "0")
            //                Task.WhenAll(
            //                    Task.Delay(1000),
            //                    Task.Run(() => contactsWithSimilarPhone.AddRange(contRepo.GetByCriteria($"query={c.custom_fields_values.First(x => x.field_id == 264911).values[0].value}")))
            //                    ).Wait();

            //    if (c.custom_fields_values.Any(x => x.field_id == 264913))
            //        foreach (var v in c.custom_fields_values.First(x => x.field_id == 264913).values)
            //            if ((string)v.value != "" &&
            //                (string)v.value != "0")
            //                Task.WhenAll(
            //                    Task.Delay(1000),
            //                    Task.Run(() => contactsWithSimilarMail.AddRange(contRepo.GetByCriteria($"query={c.custom_fields_values.First(x => x.field_id == 264913).values[0].value}")))
            //                    ).Wait();

            //    if (contactsWithSimilarPhone.Distinct(new ContactsComparer()).Count() > 1)
            //        doubleContacts.Add(((int)c.id, (string)c.custom_fields_values.First(x => x.field_id == 264911).values[0].value));
            //    if (contactsWithSimilarMail.Distinct(new ContactsComparer()).Count() > 1)
            //        doubleContacts.Add(((int)c.id, (string)c.custom_fields_values.First(x => x.field_id == 264913).values[0].value));
            //});

            //var l1 = doubleContacts.GroupBy(x => x.Item1).Select(g => new { cid = g.Key, cont = g.First().Item2 }).ToList();
            //var l2 = l1.GroupBy(x => x.cont).Select(g => new { cid = g.First().cid, cont = g.Key }).ToList();

            //using StreamWriter sw1 = new StreamWriter("ContactsWithDoubles.v2.csv", false, System.Text.Encoding.Default);
            //sw1.WriteLine($"cid;contact");
            //foreach (var c in l2)
            //{
            //    sw1.WriteLine($"{c.cid};{c.cont}");
            //}

            //return Ok();
            #endregion
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

            if (Request.Headers["x-requested-with"] == "XMLHttpRequest")
                return Ok(new { Message = "SUCCESS"});
            return Ok();
        }
    }
}