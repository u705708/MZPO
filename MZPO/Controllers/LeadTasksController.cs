using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MZPO.Controllers
{
    [Route("api/[controller]")]
    public class LeadTasksController : ControllerBase
    {
        private readonly Amo _amo;
        private readonly IAmoRepo<Lead> _leadRepo;

        public LeadTasksController(Amo amo)
        {
            _amo = amo;
            _leadRepo = amo.GetAccountById(28395871).GetRepo<Lead>();
        }

        public class TaskEntry
        {
            public int task_id { get; set; }
            public List<string> variants { get; set; }
        }

        private static List<string> GetVariants(int taskId)
        {
            return new List<string>() { "Отправлено КП", "Взято в работу", "Получена оплата" };
        }

        // GET: api/leadtasks/1
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var tasks = _leadRepo.GetEntityTasks(id);
            var taskEntries = tasks.Where(x => !x.is_completed).Select(x => new TaskEntry() { task_id = x.id, variants = GetVariants(x.id) });

            //return Ok(tasks);
            return Ok(taskEntries);
        }

        // PUT: api/leadtasks/
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