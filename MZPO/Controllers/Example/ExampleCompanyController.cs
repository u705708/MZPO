using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("example/company")]
    [ApiController]
    public class ExampleCompanyController : ControllerBase
    {
        // GET example/company
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(new Integration1C.Company());
        }
    }
}
