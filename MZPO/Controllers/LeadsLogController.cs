using System;
using Microsoft.AspNetCore.Mvc;
using MZPO.AmoRepo;
using MZPO.Services;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MZPO.Controllers
{
    [Route("log/leads")]
    [ApiController]
    public class LeadsLogController : ControllerBase
    {
        private readonly AmoAccount _acc;
        private readonly Log _log;

        public LeadsLogController(Amo amo, Log log)
        {
            _acc = amo.GetAccountById(28395871);
            _log = log;
        }
        
        
        // GET: log/leads                                                                                               //Возвращаем логи операций
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(_log.GetLog());
        }

        // GET log/leads/5
        [HttpGet("{input}")]                                                                                            //Возвращаем конкретную сделку
        public ActionResult<string> Get(string input)
        {
            if (!Int32.TryParse(input, out int id)) return BadRequest("Incorrect lead number");

            try
            {
                var leadRepo = _acc.GetRepo<Lead>();
                return Ok(JsonConvert.SerializeObject(leadRepo.GetById(id), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented }));
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}