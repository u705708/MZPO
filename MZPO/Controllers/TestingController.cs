using Microsoft.AspNetCore.Mvc;

namespace MZPO.Controllers
{
    [Route("api/testing")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        public TestingController()
        {

        }

        // GET: api/testing
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("𓅮 𓃟 ne tovarisch");
        }
    }
}