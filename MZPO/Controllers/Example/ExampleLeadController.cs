﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("example/lead")]
    [ApiController]
    public class ExampleLeadController : ControllerBase
    {
        // GET example/lead
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(new Integration1C.Lead());
        }
    }
}
