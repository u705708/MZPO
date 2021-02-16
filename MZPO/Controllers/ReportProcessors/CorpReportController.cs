using Microsoft.AspNetCore.Mvc;
using MZPO.ReportProcessors;
using MZPO.Services;
using System;
using System.Threading;

namespace MZPO.Controllers
{
    [Route("preparereports/corporate")]
    [ApiController]
    public class CorpReportController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;
        private readonly string reportName;
        private readonly string taskName;

        public CorpReportController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(19453687);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "1jzqcptdlCpSPXcyLpumSGCaHtSVi28bg8Ga2aEFXCoQ";
            reportName = "CorpReport";
            taskName = "report_corp";
        }

        // GET: preparereports/corporate
        [HttpGet]
        public ActionResult Get()
        {
            return Redirect($"https://docs.google.com/spreadsheets/d/{sheetId}/");
        }

        // GET preparereports/corporate/1609448400,1612126800
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public ActionResult Get(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<CorpReportProcessor> reportProcessor = new Lazy<CorpReportProcessor>(() =>                                                     //Создаём экземпляр процессора
                               new CorpReportProcessor(_acc, _processQueue, _gSheets, sheetId, dateFrom, dateTo, taskName, token));

            _processQueue.AddTask(reportProcessor.Value.Run(), cts, taskName, _acc.name, reportName);                                           //Запускаем его и добавляем в очередь
            return Ok();
        }
    }
}