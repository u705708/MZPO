using Microsoft.AspNetCore.Mvc;
using MZPO.Processors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace MZPO.Controllers
{
    [Route("reports/data")]
    [ApiController]
    public class AmoDataController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;

        public AmoDataController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(19453687);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "1JTAzCS89hLxI9fA3MWxiE9BSzZro3nPhyfy8931rZTk";
        }

        // GET: reports/data
        [HttpGet]
        public ActionResult Get()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<UnfinishedContactsProcessor> dataReportProcessor = new Lazy<UnfinishedContactsProcessor>(() =>                         //Создаём экземпляр процессора
                               new UnfinishedContactsProcessor(_acc, _gSheets, sheetId, _processQueue, token));

            Task task = Task.Run(() => dataReportProcessor.Value.Run());                                                //Запускаем его
            _processQueue.Add(task, cts, "report_data", _acc.name, "DataReport");                                       //И добавляем в очередь

            return Ok();
        }
    }
}