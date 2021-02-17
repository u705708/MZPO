using System;
using Microsoft.AspNetCore.Mvc;
using MZPO.Services;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MZPO.Controllers
{
    [Route("log/{action}")]
    public class LogController : ControllerBase
    {
        private readonly Log _log;
        private readonly TaskList _processQueue;

        public LogController(Log log, TaskList processQueue)
        {
            _log = log;
            _processQueue = processQueue;
        }
        
        // GET: log/leads                                                                                                   //Возвращаем логи операций
        [HttpGet]
        public ActionResult Leads()
        {
            return Ok(_log.GetLog());
        }

        // GET: log/queue
        [HttpGet]
        public ActionResult<string> Queue()                                                                                 //Возвращаем очередь обработки сделок
        {
            return Ok(JsonConvert.SerializeObject(_processQueue.GetList(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));
        }

        // GET log/queue/5
        [HttpGet("{id}")]
        public ActionResult<string> Queue(string id)                                                                        //Передаём CancellationToken по номеру сделки из очереди
        {
            try
            {
                _processQueue.Stop(id);
                return LocalRedirect("~/log/queue/");
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}