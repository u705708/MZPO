using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
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
using System.Threading;
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
            _acc = amo.GetAccountById(28395871);
            //_acc = amo.GetAccountById(19453687);
            _processQueue = processQueue;
        }

        // GET: api/<Testing>
        [HttpGet]
        public ActionResult Get()
        {
            //long dateFrom = 1606770000;
            //long dateTo = 1609448400;

            //CancellationTokenSource cts = new CancellationTokenSource();
            //CancellationToken token = cts.Token;
            //Lazy<CorpReportProcessor> corpReportProcessor = new Lazy<CorpReportProcessor>(() =>                      //Создаём экземпляр процессора сделки
            //                   new CorpReportProcessor(_acc, _processQueue, token, dateFrom, dateTo));

            //Task task = Task.Run(() => corpReportProcessor.Value.Run());                                               //Запускаем его
            //_processQueue.Add(task, cts, "0", _acc.name, "CorpReport");                                                //И добавляем в очередь
            //return Ok();

            //var contactRepo = _acc.GetRepo<Contact>();
            //var companyRepo = _acc.GetRepo<Company>();
            var leadRepo = _acc.GetRepo<Lead>();

            //return Ok(JsonConvert.SerializeObject(leadRepo.GetByCriteria("filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[created_at][from]=1606770000&filter[created_at][to]=1609448400"), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));
            //return Ok(leadRepo.GetByCriteria("filter[statuses][0][pipeline_id]=3558781&filter[statuses][0][status_id]=35001244&filter[custom_fields_values][118675][from]=1606770000&filter[custom_fields_values][118675][to]=1609448400&filter[responsible_user_id]=2375122"));
            //return Ok(JsonConvert.SerializeObject(leadRepo.GetById(27200619), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));
            //return Ok(companyRepo.GetById(1494207));
            //return Ok(contactRepo.GetById(46146799));
            
            var notes = leadRepo.GetEvents(23459475);
            var filteredNotes = notes;
            return Ok(filteredNotes);

            //string crit = @"filter[pipeline_id][0]=3198184&filter[pipeline_id][0]=3566374&filter[pipeline_id][0]=3558964&filter[pipeline_id][0]=3558991&filter[pipeline_id][0]=3558922&filter[created_at][from]=1606770000&filter[created_at][to]=1609448399&filter[responsible_user_id]=2375107";
            //string crit = @"filter[pipeline_id][0]=3198184&filter[created_at][from]=1606770000&filter[created_at][to]=1609448399&filter[responsible_user_id]=2375107";

            //return Ok(JsonConvert.SerializeObject(leadRepo.GetByCriteria(crit), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));
            //return Ok(leadRepo.GetByCriteria(crit));


            //return Ok();

            //var _list = JsonConvert.DeserializeObject<Dictionary<int, string>>(File.ReadAllText(@"todo.json"));

            //var proc = new TestProcessor(leadRepo);
            //proc.Run();

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
