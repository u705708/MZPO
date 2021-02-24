using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.LeadProcessors;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            //return Ok("𓅮 𓃟 ne tovarisch");

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

            var _acc = _amo.GetAccountById(28395871);

            var _compRepo = _acc.GetRepo<Company>();

            var data = _compRepo.GetById(30922773);

            return Ok(JsonConvert.SerializeObject(data, Formatting.Indented));
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