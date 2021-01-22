using Microsoft.AspNetCore.Mvc;
using MZPO.Processors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace MZPO.Controllers
{
    [Route("reports/corporate")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;

        public ReportController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(19453687);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "1OTrCdmjYRCKKdr64wLY46Rx_yAffx7li4jSxzz2C4mc";
        }

        // GET: reports/corporate
        [HttpGet]
        public ActionResult Get()
        {
            return Redirect($"https://docs.google.com/spreadsheets/d/{sheetId}/");
        }

        // GET reports/corporate/1606770000,1609448400
        [HttpGet("{from},{to}")]                                                                                        //Запрашиваем отчёт для диапазона дат
        public ActionResult Get(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<CorpReportProcessor> corpReportProcessor = new Lazy<CorpReportProcessor>(() =>                         //Создаём экземпляр процессора
                               new CorpReportProcessor(_acc, _processQueue, _gSheets, sheetId, token, dateFrom, dateTo));

            Task task = Task.Run(() => corpReportProcessor.Value.Run());                                                //Запускаем его
            _processQueue.Add(task, cts, "report_corp", _acc.name, "CorpReport");                                       //И добавляем в очередь
            return Ok();
        }
    }
}