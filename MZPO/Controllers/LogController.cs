using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MZPO.Services;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MZPO.Controllers
{
    [Route("[controller]/[action]")]
    public class LogController : ControllerBase
    {
        private readonly Log _log;
        private readonly ProcessQueue _processQueue;

        public LogController(Log log, ProcessQueue processQueue)
        {
            _log = log;
            _processQueue = processQueue;
        }

        // GET: log/leads                                                                                                   //Возвращаем логи операций
        [ActionName("Leads")]
        [HttpGet]
        public IActionResult Leads()
        {
            return Ok(_log.GetLog());
        }

        // GET: log/queue
        [ActionName("Queue")]
        [HttpGet]
        public IActionResult Queue()                                                                                 //Возвращаем очередь обработки сделок
        {
            return Ok(JsonConvert.SerializeObject(_processQueue.GetList(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));
        }

        // GET log/queue/5
        [ActionName("Queue")]
        [HttpGet("{id}")]
        public IActionResult Queue(string id)                                                                        //Передаём CancellationToken по номеру сделки из очереди
        {
            try
            {
                Task.Run(() => _processQueue.Stop(id));
                return LocalRedirect("~/log/queue/");
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}