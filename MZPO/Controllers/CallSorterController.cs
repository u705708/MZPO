using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MZPO.Services;

namespace MZPO.Controllers
{
    [Route("api/callsorter")]
    [ApiController]
    public class CallSorterController : ControllerBase
    {
        private readonly CallSorter _callSorter;

        public CallSorterController(CallSorter callSorter)
        {
            _callSorter = callSorter;
        }

        //Вообще дичь! Сервис телефонии, который используем не позволяет разделять звонок по очереди на две разных группы абонентов.
        //Тупо нет такой функции, пытались обойти через виртуальных пользователей, к каждому из которых привязана группа менеджеров,
        //но очередь, в которой реализовано равномерное распределение, не умеет распределять по виртуальным пользователям.
        //По крайней мере у сервиса есть возможность фильтровать звонки по http-запросу - сервис шлёт запрос на сервер и получает
        //вариант, по какой ветке вести звонок.Вот и приходится по очереди отдавать то 1, то 0. Тик-так, блять.

        // GET: api/callsorter
        [HttpGet]
        public ActionResult Get()
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress.ToString();

            if (remoteIp == "212.193.100.155" || remoteIp == "46.48.56.153")
                return Ok(new { choice = _callSorter.GetChoice() });
            return Unauthorized();
        }
    }
}