using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("example/client")]
    [ApiController]
    public class ExampleClientController : ControllerBase
    {
        // GET example/client
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(new Integration1C.Client());
        }
    }
}
