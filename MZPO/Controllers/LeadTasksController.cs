using Microsoft.AspNetCore.Mvc;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace MZPO.Controllers
{
    [Route("api/leadtasks")]
    public class LeadTaskController : ControllerBase
    {
        public LeadTaskController()
        {
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

        // PUT: api/leadtasks/1
        [HttpPut]
        public IActionResult Put()
        {
            using StreamReader sr = new(Request.Body);
            var hook = sr.ReadToEndAsync().Result;

            using StreamWriter sw = new("leadtask.txt", true, System.Text.Encoding.Default);
            sw.WriteLine($"--{DateTime.Now}----------------------------");
            sw.WriteLine(WebUtility.UrlDecode(hook));
            sw.WriteLine();

            if (Request.Headers["x-requested-with"] == "XMLHttpRequest")
                return Ok(new { Message = "SUCCESS" });
            return Ok();
        }
    }
}