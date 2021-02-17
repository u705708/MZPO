using Microsoft.AspNetCore.Mvc;
using MZPO.ReportProcessors;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Controllers.ReportProcessors
{
    [Route("reports/{action}")]
    public class ReportsController : Controller
    {
        private readonly TaskList _processQueue;
        private readonly Amo _amo;
        private readonly GSheets _gSheets;

        public ReportsController(Amo amo, TaskList processQueue, GSheets gSheets)
        {
            _amo = amo;
            _processQueue = processQueue;
            _gSheets = gSheets;
        }

        // GET: reports/corporatesales
        [HttpGet]
        public ActionResult CorporateSales()
        {
            return Redirect($"https://docs.google.com/spreadsheets/d/1jzqcptdlCpSPXcyLpumSGCaHtSVi28bg8Ga2aEFXCoQ/");
        }

        // GET reports/corporatesales/1609448400,1612126800
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public ActionResult CorporateSales(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.CorporateSales, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET: reports/kpi
        [HttpGet]
        public ActionResult KPI()
        {
            var yesterday = DateTime.Today.AddSeconds(-1).AddHours(2);                                                                          //Поправить на использование UTC
            var firstDayofMonth = new DateTime(yesterday.Year, yesterday.Month, 1, 2, 0, 0);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET reports/kpi/1612126799,1612886399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public ActionResult KPI(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET: reports/longleads
        [HttpGet]
        public ActionResult LongLeads()
        {
            var yesterday = DateTime.Today.AddSeconds(-1).AddHours(2);                                                                          //Поправить на использование UTC
            var firstDayofMonth = new DateTime(yesterday.Year, yesterday.Month, 1, 2, 0, 0);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dateFrom, dateTo);
            
            return Ok();
        }

        // GET reports/longleads/1610294400,1612886399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public ActionResult LongLeads(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET: reports/unfinishedcompanies
        [HttpGet]
        public ActionResult UnfinishedCompanies()
        {
            ReportsProvider.StartReport(Reports.UnfinishedCompanies, _amo, _processQueue, _gSheets, 0, 0);

            return Ok();
        }

        // GET: reports/weeklyreport/
        [HttpGet]
        public ActionResult WeeklyReport()
        {
            var yesterday = DateTime.Today.AddSeconds(-1).AddHours(2);                                                                          //Поправить на использование UTC
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, 0, dateTo);

            return Ok();
        }

        // GET reports/weeklyreport/1612126799
        [HttpGet("{to}")]                                                                                                                       //Запрашиваем отчёт для диапазона дат
        public ActionResult WeeklyReport(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, 0, dateTo);

            return Ok();
        }
    }
}