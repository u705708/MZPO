using Microsoft.AspNetCore.Mvc;
using MZPO.Processors;
using MZPO.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers
{
    [Route("reports/corporate")]
    [ApiController]
    public class CorpReportController : ControllerBase
    {
        private readonly TaskList _processQueue;
        private readonly AmoAccount _acc;
        private readonly GSheets _gSheets;
        private readonly string sheetId;

        public CorpReportController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _acc = amo.GetAccountById(19453687);
            _processQueue = processQueue;
            _gSheets = gSheets;
            sheetId = "1jzqcptdlCpSPXcyLpumSGCaHtSVi28bg8Ga2aEFXCoQ";
        }

        // GET: reports/corporate
        [HttpGet]
        public ActionResult Get()
        {
            return Redirect($"https://docs.google.com/spreadsheets/d/{sheetId}/");
        }

        // GET reports/corporate/1609448400,1612126800
        [HttpGet("{from},{to}")]                                                                                        //Запрашиваем отчёт для диапазона дат
        public ActionResult Get(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<CorpReportProcessor> corpReportProcessor = new Lazy<CorpReportProcessor>(() =>                         //Создаём экземпляр процессора
                               new CorpReportProcessor(_acc, _processQueue, _gSheets, sheetId, dateFrom, dateTo, token));

            Task task = Task.Run(() => corpReportProcessor.Value.Run());                                                //Запускаем его
            _processQueue.AddTask(task, cts, "report_corp", _acc.name, "CorpReport");                                       //И добавляем в очередь
            return Ok();
        }
    }
}