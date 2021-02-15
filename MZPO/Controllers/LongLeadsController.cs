using Microsoft.AspNetCore.Mvc;
using MZPO.ReportProcessors;
using MZPO.Services;
using System;
using System.Threading;

namespace MZPO.Controllers
{
    [Route("preparereports/longleads")]
    [ApiController]
    public class LongLeadsController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;
        private readonly string reportName;
        private readonly string taskName;

        public LongLeadsController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(28395871);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "1EtpEiq5meigVrY9-n3phHxQRVO3iHgpF6V0-wpJ5Yg4";
            reportName = "LongLeadsReport";
            taskName = "report_long";
        }

        // GET: preparereports/longleads
        [HttpGet]
        public ActionResult Get()
        {
            var yesterday = DateTime.Today.AddSeconds(-1).AddHours(2);                                                                          //Поправить на использование UTC
            var firstDayofMonth = new DateTime(yesterday.Year, yesterday.Month, 1, 2, 0, 0);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IProcessor> reportProcessor = new Lazy<IProcessor>(() =>                                                                       //Создаём экземпляр процессора
                               new LongLeadsProcessor(_acc, _gSheets, sheetId, _processQueue, dateFrom, dateTo, taskName, token));

            _processQueue.AddTask(reportProcessor.Value.Run(), cts, taskName, _acc.name, reportName);                                           //Запускаем его и добавляем в очередь
            return Ok();
        }

        // GET preparereports/longleads/1610294400,1612886399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public ActionResult Get(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IProcessor> reportProcessor = new Lazy<IProcessor>(() =>                                                                       //Создаём экземпляр процессора
                               new LongLeadsProcessor(_acc, _gSheets, sheetId, _processQueue, dateFrom, dateTo, taskName, token));

            _processQueue.AddTask(reportProcessor.Value.Run(), cts, taskName, _acc.name, reportName);                                           //Запускаем его и добавляем в очередь
            return Ok();
        }
    }
}