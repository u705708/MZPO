using Microsoft.AspNetCore.Mvc;
using MZPO.Services;
using Newtonsoft.Json;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MZPO.Controllers
{
    [Route("log/queue/")]
    [ApiController]
    public class QueueLogController : ControllerBase
    {
        private readonly TaskList _processQueue;

        public QueueLogController(TaskList processQueue)
        {
            _processQueue = processQueue;
        }
        // GET: log/queue
        [HttpGet]
        public ActionResult<string> Get()                                                                               //Возвращаем очередь обработки сделок
        {
            return Ok(JsonConvert.SerializeObject(_processQueue.GetList(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));
        }

        // GET log/queue/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)                                                                      //Передаём CancellationToken по номеру сделки из очереди
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
