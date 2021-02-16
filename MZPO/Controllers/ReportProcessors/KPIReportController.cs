using Microsoft.AspNetCore.Mvc;
using MZPO.ReportProcessors;
using MZPO.Services;
using System;
using System.Threading;

namespace MZPO.Controllers
{
    [Route("preparereports/kpi")]
    [ApiController]
    public class KPIReportController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;
        private readonly string reportName;
        private readonly string taskName;

        public KPIReportController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(28395871);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "1ZjdabzAtTQKKdK5ZtGfvYT2jA-JN6agO0QMxtWPed0k";
            reportName = "KPIReport";
            taskName = "report_kpi";
        }

        // GET: preparereports/kpi
        [HttpGet]
        public ActionResult Get()
        {
            var yesterday = DateTime.Today.AddSeconds(-1).AddHours(2);                                                                          //Поправить на использование UTC
            var firstDayofMonth = new DateTime(yesterday.Year, yesterday.Month, 1, 2, 0, 0);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IReportProcessor> reportProcessor = new Lazy<IReportProcessor>(() =>                                                                       //Создаём экземпляр процессора
                               new RetailKPIProcessor(_acc, _gSheets, sheetId, _processQueue, dateFrom, dateTo, taskName, token));

            _processQueue.AddTask(reportProcessor.Value.Run(), cts, taskName, _acc.name, reportName);                                           //Запускаем его и добавляем в очередь
            return Ok();
        }

        // GET reports/kpi/1612126799,1612886399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public ActionResult Get(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IReportProcessor> reportProcessor = new Lazy<IReportProcessor>(() =>                                                                       //Создаём экземпляр процессора
                               new RetailKPIProcessor(_acc, _gSheets, sheetId, _processQueue, dateFrom, dateTo, taskName, token));

            _processQueue.AddTask(reportProcessor.Value.Run(), cts, taskName, _acc.name, reportName);                                           //Запускаем его и добавляем в очередь
            return Ok();
        }
    }
}