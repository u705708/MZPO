using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MZPO.Data;
using MZPO.Processors;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MZPO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Testing : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly AmoAccount _acc;

        public Testing(Amo amo, TaskList processQueue)
        {
            _amo = amo;
            //_acc = amo.GetAccountById(28395871);
            _processQueue = processQueue;
        }

        // GET: api/<Testing>
        [HttpGet]
        public ActionResult Get()
        {
            //var leadRepo = _acc.GetRepo<Lead>();

            //return Ok(JsonConvert.SerializeObject(leadRepo.GetByCriteria("filter[cf][639081]=www.skillbank.su"), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));

            //return Ok(leadRepo.GetById(23154053));

            return Ok();

            //var _list = JsonConvert.DeserializeObject<Dictionary<int, string>>(File.ReadAllText(@"todo.json"));

            //var proc = new TestProcessor(leadRepo);
            //proc.Run();

            //return Ok();
        }

        // GET api/<Testing>/5
        //[HttpGet("{input}")]
        //public FileStreamResult Get(string input)
        //{
        //    var stream = new FileStream("report.xls", FileMode.Open);
        //    return new FileStreamResult(stream, "application/vnd.ms-excel")
        //    {
        //        FileDownloadName = "report.xls"
        //    };
        //}

        //POST api/<Testing>
        [HttpPost]
        public ActionResult Post()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new StreamWriter("hook.txt", true, System.Text.Encoding.Default);
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine("--**--**--");

            //var col = Request.Form;
            //int leadNumber = 0;

            //if (col.ContainsKey("leads[add][0][id]")) Int32.TryParse(col["leads[add][0][id]"], out leadNumber);
            //else if (col.ContainsKey("unsorted[update][0][data][leads][0][id]")) Int32.TryParse(col["unsorted[update][0][data][leads][0][id]"], out leadNumber);
            //string unsorted_id = col["unsorted[add][0][uid]"];
            //sw.WriteLine($"leadNumber: {leadNumber}, unsorted_id: {unsorted_id}");
            //sw.WriteLine("--**--**--");

            return Ok();
        }
    }
}
