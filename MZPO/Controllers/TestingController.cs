using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.LeadProcessors;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public TestingController(Amo amo, TaskList processQueue, GSheets gSheets, Log log)
        {
            _amo = amo;
            _processQueue = processQueue;
            _gSheets = gSheets;
            _log = log;
        }


        public class Entry
        {
            public int payment_received;
            public int payment_amount;
            public string receipt_number;
            public string manager;
            public string client_name;
        }

        // GET: api/testing
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("𓅮 𓃟 ne tovarisch");

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

            //List<(int?, int)> managersLeadsList = new();

            //Parallel.ForEach(leadIdList, l =>
            //{
            //    var lead = leadRepo.GetById(l.Item1);
            //    if (lead.pipeline_id != 3558922) return;
            //    managersLeadsList.Add((l.Item2, lead.id));
            //});

            //List<(string, int)> totals = new();

            //foreach (var m in managers)
            //    totals.Add((m.Item2, managersLeadsList.Count(x => x.Item1 == m.Item1)));

            //return Ok(JsonConvert.SerializeObject(totals, Formatting.Indented));
        }

        // POST: api/testing
        public ActionResult Post()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            return Ok();
        }
    }
}