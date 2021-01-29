using Microsoft.AspNetCore.Mvc;
using MZPO.Processors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            _processQueue = processQueue;
            _gSheets = gSheets;

            //_acc = amo.GetAccountById(19453687); //corp
            //sheetId = "1JTAzCS89hLxI9fA3MWxiE9BSzZro3nPhyfy8931rZTk"; //UnfinishedContacts

            _acc = amo.GetAccountById(28395871); //retail
            sheetId = "1ZjdabzAtTQKKdK5ZtGfvYT2jA-JN6agO0QMxtWPed0k"; //RetailKPI
        }

        // GET: reports/data
        [HttpGet]
        public ActionResult Get()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IProcessor> dataReportProcessor = new Lazy<IProcessor>(() =>                         //Создаём экземпляр процессора
                               new RetailKPIProcessor(_acc, _gSheets, sheetId, _processQueue, token));

            Task task = Task.Run(() => dataReportProcessor.Value.Run());                                                //Запускаем его
            _processQueue.Add(task, cts, "report_data", _acc.name, "DataReport");                                       //И добавляем в очередь

            return Ok();
        }
    }
}