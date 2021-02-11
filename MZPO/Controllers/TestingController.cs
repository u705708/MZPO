using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.Processors;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("api/testing")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;

        public TestingController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(28395871);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "";
        }

        // GET: api/testing
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("𓅮 𓃟 ne tovarisch");
        }

        [HttpGet("{input}")]
        public ActionResult<string> Get(string input)
        {
            if (!Int32.TryParse(input, out int id)) return BadRequest("Incorrect lead number");

            var leadRepo = _acc.GetRepo<Lead>();
            var contRepo = _acc.GetRepo<Contact>();

            var lead = leadRepo.GetById(id);

            List<int> replyTimestamps = new List<int>();

            int timeOfReference = (int)lead.created_at;

            #region Результат звонка
            if (lead.custom_fields_values is not null)
            {
                var cf = lead.custom_fields_values.FirstOrDefault(x => x.field_id == 644675);
                if (cf is not null)
                {
                    var cfValue = (string)cf.values[0].value;
                    if (cfValue == "Принят" || cfValue == "Ручная сделка") return Ok(0);
                }
            }
            #endregion

            #region Время суток
            var dt = DateTimeOffset.FromUnixTimeSeconds(timeOfReference).UtcDateTime;
            if (dt.Hour > 17)
                timeOfReference = (int)((DateTimeOffset)new DateTime(dt.Year, dt.Month, dt.Day, 11, 0, 0).AddDays(1)).ToUnixTimeSeconds();
            else if (dt.Hour < 6)
                timeOfReference = (int)((DateTimeOffset)new DateTime(dt.Year, dt.Month, dt.Day, 11, 0, 0)).ToUnixTimeSeconds();
            #endregion

            var allEvents = new List<Event>();
            var allNotes = new List<Note>();

            allEvents.AddRange(leadRepo.GetEntityEvents(lead.id));
            allNotes.AddRange(leadRepo.GetEntityNotes(lead.id));

            #region Смена ответственного
            if (allEvents
                    .Where(x => x.type == "entity_responsible_changed")
                    .Any(x => x.value_before[0].responsible_user.id == 2576764 &&                                   //Если меняли ответственного с Администартора на текущего
                        x.value_after[0].responsible_user.id == lead.responsible_user_id))
                timeOfReference = (int)allEvents
                    .Where((x => x.type == "entity_responsible_changed"))
                    .First(x => x.value_before[0].responsible_user.id == 2576764)
                    .created_at;
            else if(allEvents
                    .Where(x => x.type == "entity_responsible_changed")                                             //Если меняли на текущего
                    .Any(x => x.value_after[0].responsible_user.id == lead.responsible_user_id) &&
                    allEvents
                    .Where(x => x.type == "entity_responsible_changed")                                             //И с Администратора
                    .Any(x => x.value_before[0].responsible_user.id == 2576764))
                timeOfReference = (int)allEvents
                    .Where((x => x.type == "entity_responsible_changed"))
                    .First(x => x.value_before[0].responsible_user.id == 2576764)
                    .created_at;
            #endregion

            #region Собираем данные из контактов
            if (lead._embedded.contacts is not null)
                Parallel.ForEach(lead._embedded.contacts, contact =>
                {
                    var events = contRepo.GetEntityEvents(contact.id);
                    lock (allEvents)
                    {
                        allEvents.AddRange(events);
                    }
                    var notes = contRepo.GetEntityNotes(contact.id);
                    lock (allNotes)
                    {
                        allNotes.AddRange(notes);
                    }
                });
            #endregion

            #region Cообщения в чат
            foreach (var e in allEvents)
                if ((e.type == "outgoing_chat_message") || (e.type == "incoming_chat_message"))
                    replyTimestamps.Add((int)e.created_at);
            #endregion

            #region Исходящее письмо
            foreach (var n in allNotes)
                if ((n.note_type == "amomail_message") && (n.parameters.income == false))
                    replyTimestamps.Add((int)n.created_at);
            #endregion

            #region Звонки
            foreach (var e in allEvents)
            {
                if ((e.type == "outgoing_call") || (e.type == "incoming_call"))
                {
                    Note callNote;
                    
                    if (allNotes.Any(x => x.id == e.value_after[0].note.id))
                        callNote = allNotes.First(x => x.id == e.value_after[0].note.id);
                    else callNote = contRepo.GetNoteById(e.value_after[0].note.id);
                    
                    int duration = 0;

                    if (callNote.parameters is not null && callNote.parameters.duration > 0)
                        duration = (int)callNote.parameters.duration;

                    int actualCallTime = (int)e.created_at - duration;

                    if ((e.type == "outgoing_call") && (actualCallTime > lead.created_at))
                        replyTimestamps.Add(actualCallTime);
                    else if ((duration > 0) && (actualCallTime > lead.created_at))
                        replyTimestamps.Add(actualCallTime);
                }
            }
            #endregion

            replyTimestamps.Add(timeOfReference + 86400);
            int result = replyTimestamps.Select(x => x - timeOfReference).Where(x => x > -600).Min();

            return Ok(result);
        }
    }
}