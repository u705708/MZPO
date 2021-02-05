using Microsoft.AspNetCore.Mvc;
using MZPO.Processors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("reports/kpi")]
    [ApiController]
    public class KPIReportController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;

        public KPIReportController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(28395871);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "1ZjdabzAtTQKKdK5ZtGfvYT2jA-JN6agO0QMxtWPed0k";
        }

        // GET: reports/kpi
        [HttpGet]
        public ActionResult Get()
        {
            var yesterday = DateTime.Today.AddSeconds(-1).AddHours(2);      //Поправить на использование UTC
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IProcessor> reportProcessor = new Lazy<IProcessor>(() =>                         //Создаём экземпляр процессора
                               new RetailKPIProcessor(_acc, _gSheets, sheetId, _processQueue, dateTo, token));

            Task task = Task.Run(() => reportProcessor.Value.Run());                                                //Запускаем его
            _processQueue.AddTask(task, cts, "report_kpi", _acc.name, "KPIReport");                                       //И добавляем в очередь
            return Ok();
        }

        // GET reports/kpi/1612126799
        [HttpGet("{to}")]                                                                                        //Запрашиваем отчёт для диапазона дат
        public ActionResult Get(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IProcessor> reportProcessor = new Lazy<IProcessor>(() =>                         //Создаём экземпляр процессора
                               new RetailKPIProcessor(_acc, _gSheets, sheetId, _processQueue, dateTo, token));

            Task task = Task.Run(() => reportProcessor.Value.Run());                                                //Запускаем его
            _processQueue.AddTask(task, cts, "report_kpi", _acc.name, "KPIReport");                                       //И добавляем в очередь
            return Ok();
        }
    }
}