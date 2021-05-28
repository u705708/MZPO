using Microsoft.AspNetCore.Mvc;
using MZPO.Services;
using System.Collections.Generic;

namespace MZPO.Controllers
{
    [Route("api/leadtasks")]
    public class LeadTaskController : ControllerBase
    {
        private readonly LeadsSorter _callSorter;

        public LeadTaskController(LeadsSorter callSorter)
        {
            _callSorter = callSorter;
        }

        public class TaskEntry
        {
            public int task_id { get; set; }
            public List<string> variants { get; set; }
        }

        // GET: api/leadtasks/1
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            List<TaskEntry> te = new() {
                new() { 
                    task_id = 12433799,
                    variants = new() { "Отправлено КП", "Взято в работу", "Получена оплата" }
                },
                new() {
                    task_id = 12433551,
                    variants = new() { "СПАМ", "Нецелевой контакт", "Выставлен счёт" }
                },
            };
            return Ok(te);
        }
    }
}