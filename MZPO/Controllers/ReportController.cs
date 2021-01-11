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

        public ReportController(Amo amo, TaskList processQueue)
        {
            _acc = amo.GetAccountById(19453687);
            _processQueue = processQueue;
        }

        // GET: reports/corporate
        [HttpGet]
        public FileStreamResult Get()
        {
            var stream = new FileStream("report.xlsx", FileMode.Open);
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "report.xlsx"
            };
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
                               new CorpReportProcessor(_acc, _processQueue, token, dateFrom, dateTo));

            Task task = Task.Run(() => corpReportProcessor.Value.Run());                                                //Запускаем его
            _processQueue.Add(task, cts, "report_corp", _acc.name, "CorpReport");                                       //И добавляем в очередь
            return Ok();
        }
    }
}
