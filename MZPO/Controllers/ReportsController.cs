using Microsoft.AspNetCore.Mvc;
using MZPO.ReportProcessors;
using MZPO.Services;
using System;

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
        public IActionResult CorporateSales()
        {
            return Redirect($"https://docs.google.com/spreadsheets/d/1jzqcptdlCpSPXcyLpumSGCaHtSVi28bg8Ga2aEFXCoQ/");
        }

        // GET reports/corporatesales/1614546000,1617224399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult CorporateSales(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.CorporateSales, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET: reports/kpi
        [HttpGet]
        public IActionResult KPI()
        {
            var now = DateTime.UtcNow.AddHours(3);
            var yesterday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            var firstDayofMonth = new DateTime(yesterday.Year, yesterday.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET reports/kpi/1612126799,1612886399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult KPI(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET: reports/longleads
        [HttpGet]
        public IActionResult LongLeads()
        {
            var now = DateTime.UtcNow.AddHours(3);
            var yesterday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            var firstDayofMonth = new DateTime(yesterday.Year, yesterday.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dateFrom, dateTo);
            
            return Ok();
        }

        // GET reports/longleads/1614546000,1617224399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult LongLeads(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET: reports/unfinishedcompanies
        [HttpGet]
        public IActionResult UnfinishedCompanies()
        {
            ReportsProvider.StartReport(Reports.UnfinishedCompanies, _amo, _processQueue, _gSheets, 0, 0);

            return Ok();
        }

        // GET: reports/weeklyreport/
        [HttpGet]
        public IActionResult WeeklyReport()
        {
            var now = DateTime.UtcNow.AddHours(3);
            var yesterday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, 0, dateTo);

            return Ok();
        }

        // GET reports/weeklyreport/1612126799
        [HttpGet("{to}")]                                                                                                                       //Запрашиваем отчёт для диапазона дат
        public IActionResult WeeklyReport(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, 0, dateTo);

            return Ok();
        }

        // GET: reports/monthlyreport/
        [HttpGet]
        public IActionResult MonthlyReport()
        {
            var now = DateTime.UtcNow.AddHours(3);
            var yesterday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            var firstDayofMonth = new DateTime(yesterday.Year, yesterday.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            ReportsProvider.StartReport(Reports.KPI_monthly, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }

        // GET reports/monthlyreport/1614546000,1617224399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult MonthlyReport(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.KPI_monthly, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok();
        }
    }
}