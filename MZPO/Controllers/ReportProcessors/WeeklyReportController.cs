using Microsoft.AspNetCore.Mvc;
using MZPO.ReportProcessors;
using MZPO.Services;
using System;
using System.Threading;

namespace MZPO.Controllers
{
    [Route("preparereports/weekly")]
    [ApiController]
    public class WeeklyReportController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;
        private readonly string reportName;
        private readonly string taskName;

        public WeeklyReportController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(28395871);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "1Am4JA46Nbaa1GxOgeKbhKRMWXkkzRS2SoZBhVjqqueY";
            reportName = "RetailReport";
            taskName = "report_retail";
        }

        // GET: preparereports/weekly/
        [HttpGet]
        public ActionResult Get()
        {
            var yesterday = DateTime.Today.AddSeconds(-1).AddHours(2);                                                                          //Поправить на использование UTC
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IReportProcessor> reportProcessor = new Lazy<IReportProcessor>(() =>                                                                       //Создаём экземпляр процессора
                               new WeeklyKPIReportProcessor(_acc, _gSheets, sheetId, _processQueue, dateTo, taskName, token));

            _processQueue.AddTask(reportProcessor.Value.Run(), cts, taskName, _acc.name, reportName);                                           //Запускаем его и добавляем в очередь
            return Ok();
        }

        // GET preparereports/weekly/1612126799
        [HttpGet("{to}")]                                                                                                                       //Запрашиваем отчёт для диапазона дат
        public ActionResult Get(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IReportProcessor> reportProcessor = new Lazy<IReportProcessor>(() =>                                                                       //Создаём экземпляр процессора
                               new WeeklyKPIReportProcessor(_acc, _gSheets, sheetId, _processQueue, dateTo, taskName, token));

            _processQueue.AddTask(reportProcessor.Value.Run(), cts, taskName, _acc.name, reportName);                                           //Запускаем его и добавляем в очередь
            return Ok();
        }
    }
}