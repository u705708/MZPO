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
            //var companyRepo = _acc.GetRepo<Company>();
            var leadRepo = _acc.GetRepo<Lead>();
            var contactRepo = _acc.GetRepo<Contact>();

            //return Ok(JsonConvert.SerializeObject(leadRepo.GetById(27200619), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));

            //var notes = contactRepo.GetEvents(32858435);
            //var notes = contactRepo.GetNotes(32858435);
            //var notes = leadRepo.GetEvents(23464575);
            //var notes = leadRepo.GetNotes(23465041);

            return Ok();
        }

        // GET api/<Testing>/5
        [HttpGet("{input}")]
        public ActionResult Get(string input)
        {
            //if (!Int32.TryParse(input, out int leadId)) return BadRequest("Incorrect lead number");

            //var leadRepo = _acc.GetRepo<Lead>();
            //var contactRepo = _acc.GetRepo<Contact>();

            //List<int> replyTimestamps = new List<int>();
            //Lead lead = leadRepo.GetById(leadId);

            //int timeOfReference = (int)lead.created_at;

            //#region Результат звонка
            //if (lead.custom_fields_values != null)
            //{
            //    var cf = lead.custom_fields_values.FirstOrDefault(x => x.field_id == 644675);
            //    if (cf != null)
            //    {
            //        var cfValue = (string)cf.values[0].value;
            //        if (cfValue == "Принят" || cfValue == "Ручная сделка") return Ok("0");
            //    }
            //}
            //#endregion

            //var leadEvents = leadRepo.GetEvents(leadId);

            //#region Смена ответственного
            //if ((leadEvents != null) &&
            //    leadEvents.Where((x => x.type == "entity_responsible_changed")).Any(x => x.value_before[0].responsible_user.id == 2576764))
            //    timeOfReference = (int)leadEvents.Where((x => x.type == "entity_responsible_changed")).First(x => x.value_before[0].responsible_user.id == 2576764).created_at;
            //#endregion

            //#region Исходящие сообщения в чат
            //if (leadEvents != null)
            //    foreach (var e in leadEvents)
            //        if ((e.type == "outgoing_chat_message") || (e.type == "incoming_chat_message"))
            //            replyTimestamps.Add((int)e.created_at);
            //#endregion

            //#region Исходящее письмо
            //var notes = leadRepo.GetNotes(leadId);
            //if (notes != null)
            //    foreach (var n in notes)
            //        if ((n.note_type == "amomail_message") && (n.parameters.income == false))
            //            replyTimestamps.Add((int)n.created_at);
            //#endregion

            //#region Звонки
            //if (lead._embedded.contacts != null)
            //    foreach (var c in lead._embedded.contacts)
            //    {
            //        var contactEvents = contactRepo.GetEvents(c.id);
            //        if (contactEvents != null)
            //            foreach (var e in contactEvents)
            //            {
            //                if ((e.type == "outgoing_call") || (e.type == "incoming_call"))
            //                {
            //                    var callNote = contactRepo.GetNoteById(e.value_after[0].note.id);
            //                    int duration = 0;

            //                    if (callNote.parameters != null && callNote.parameters.duration > 0)
            //                        duration = (int)callNote.parameters.duration;

            //                    int actualCallTime = (int)e.created_at - duration;

            //                    if ((e.type == "outgoing_call") && (actualCallTime > lead.created_at))
            //                        replyTimestamps.Add(actualCallTime);
            //                    else if ((duration > 0) && (actualCallTime > lead.created_at))
            //                    {
            //                        replyTimestamps.Add(actualCallTime);
            //                    }
            //                }
            //            }
            //    }
            //#endregion

            //replyTimestamps.Add(timeOfReference + 86400);
            //int result = replyTimestamps.Select(x => x - timeOfReference).Where(x => x > -600).Min();

            //return Ok(result);
            
            return Ok();
        }

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
