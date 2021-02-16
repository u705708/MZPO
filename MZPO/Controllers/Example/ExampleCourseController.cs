using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("example/course")]
    [ApiController]
    public class ExampleCourseController : ControllerBase
    {
        // GET example/course
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(new Integration1C.Course());
        }
    }
}
