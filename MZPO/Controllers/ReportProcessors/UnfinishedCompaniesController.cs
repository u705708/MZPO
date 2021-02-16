using Microsoft.AspNetCore.Mvc;
using MZPO.ReportProcessors;
using MZPO.Services;
using System;
using System.Threading;

namespace MZPO.Controllers
{
    [Route("preparereports/unfinished")]
    [ApiController]
    public class UnfinishedCompaniesController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;
        private readonly string reportName;
        private readonly string taskName;

        public UnfinishedCompaniesController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _processQueue = processQueue;
            _gSheets = gSheets;

            _acc = amo.GetAccountById(19453687);
            sheetId = "1JTAzCS89hLxI9fA3MWxiE9BSzZro3nPhyfy8931rZTk";
            reportName = "CorpReport Unfinished contacts";
            taskName = "report_corp_unfinished";
        }

        // GET: preparereports/unfinished
        [HttpGet]
        public ActionResult Get()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IReportProcessor> reportProcessor = new Lazy<IReportProcessor>(() =>                                                                       //Создаём экземпляр процессора
                               new UnfinishedContactsProcessor(_acc, _gSheets, sheetId, _processQueue, taskName, token));

            _processQueue.AddTask(reportProcessor.Value.Run(), cts, taskName, _acc.name, reportName);                                           //Запускаем его и добавляем в очередь

            return Ok();
        }
    }
}