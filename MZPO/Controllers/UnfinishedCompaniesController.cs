using Microsoft.AspNetCore.Mvc;
using MZPO.Processors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        public UnfinishedCompaniesController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _processQueue = processQueue;
            _gSheets = gSheets;

            _acc = amo.GetAccountById(19453687); //corp
            sheetId = "1JTAzCS89hLxI9fA3MWxiE9BSzZro3nPhyfy8931rZTk"; //UnfinishedContacts
        }

        // GET: preparereports/unfinished
        [HttpGet]
        public ActionResult Get()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IProcessor> dataReportProcessor = new Lazy<IProcessor>(() =>                         //Создаём экземпляр процессора
                               new UnfinishedContactsProcessor(_acc, _gSheets, sheetId, _processQueue, token));

            Task task = Task.Run(() => dataReportProcessor.Value.Run());                                                //Запускаем его
            _processQueue.AddTask(task, cts, "report_data", _acc.name, "DataReport");                                       //И добавляем в очередь

            return Ok();
        }
    }
}