using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Controllers.Example
{
    [Route("example/{action}")]
    public class ExampleController : Controller
    {
        // GET example/client
        [HttpGet]
        public ActionResult Client()
        {
            return Ok(new Integration1C.Client1C() { amo_ids = new() { new() } });
        }

        // GET example/company
        [HttpGet]
        public ActionResult Company()
        {
            return Ok(new Integration1C.Company1C() { amo_ids = new() { new() } });
        }

        // GET example/course
        [HttpGet]
        public ActionResult Course()
        {
            return Ok(new Integration1C.Course1C() { amo_ids = new() { new() } });
        }

        // GET example/lead
        [HttpGet]
        public ActionResult Lead()
        {
            return Ok(new Integration1C.Lead1C() { amo_ids = new() { new() } });
        }
    }
}
