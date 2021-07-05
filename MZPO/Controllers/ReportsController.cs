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

        private static (long, long) GetDates()
        {
            var now = DateTime.UtcNow.AddHours(3);
            var yesterday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            var firstDayofMonth = new DateTime(yesterday.Year, yesterday.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(-3);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            return (dateFrom, dateTo);
        }

        private static (long, long) GetDates(long date)
        {
            var enddate = DateTimeOffset.FromUnixTimeSeconds(date).UtcDateTime.AddHours(3);
            var firstDayofMonth = new DateTime(enddate.Year, enddate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(-3);
            long dateFrom = ((DateTimeOffset)firstDayofMonth).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)enddate).ToUnixTimeSeconds();

            return (dateFrom, dateTo);
        }

        // GET: reports/unfinishedcompanies
        [HttpGet]
        public IActionResult UnfinishedCompanies()
        {
            ReportsProvider.StartReport(Reports.UnfinishedCompanies, _amo, _processQueue, _gSheets, 0, 0);

            return Ok("Requested.");
        }

        #region CorporateSales
        // GET reports/corporatesales/1619816400,1622494799
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult CorporateSales(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.CorporateSales, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        // GET reports/corporatesales/1619816399
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult CorporateSales(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.CorporateSales, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        // GET: reports/corporatesales
        [HttpGet]
        public IActionResult CorporateSales()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.CorporateSales, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region KPI
        // GET reports/kpi/1617224400,1619816399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult KPI(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        // GET reports/kpi/1619816399
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult KPI(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates();

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        // GET: reports/kpi
        [HttpGet]
        public IActionResult KPI()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region LongLeads
        // GET reports/longleads/1617224400,1619816399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult LongLeads(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        // GET reports/longleads/1619816399
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult LongLeads(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        // GET: reports/longleads
        [HttpGet]
        public IActionResult LongLeads()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region WeeklyReport
        // GET reports/weeklyreport/1617224400,1619816399
        [HttpGet("{from},{to}")]                                                                                                                       //Запрашиваем отчёт для диапазона дат
        public IActionResult WeeklyReport(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        // GET reports/weeklyreport/1619816399
        [HttpGet("{to}")]                                                                                                                       //Запрашиваем отчёт для диапазона дат
        public IActionResult WeeklyReport(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        // GET: reports/weeklyreport/
        [HttpGet]
        public IActionResult WeeklyReport()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region MonthlyReport
        // GET reports/monthlyreport/1617224400,1619816399
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult MonthlyReport(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.KPI_monthly, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        // GET reports/monthlyreport/1619816399
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult MonthlyReport(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.KPI_monthly, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        // GET: reports/monthlyreport/
        [HttpGet]
        public IActionResult MonthlyReport()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.KPI_monthly, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region DoublesList
        // GET reports/doubleslist/1622494800,1625086799
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult DoublesList(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.Doubles, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        // GET reports/doubleslist/1619816399
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult DoublesList(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.Doubles, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        // GET: reports/doubleslist/
        [HttpGet]
        public IActionResult DoublesList()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.Doubles, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region Cards
        // GET reports/cards/1619816400,1622494799
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Cards(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.Cards, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        // GET reports/cards/1622494799
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Cards(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.Cards, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        // GET: reports/cards/
        [HttpGet]
        public IActionResult Cards()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.Cards, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region Uber
        // GET reports/uber/1598907600,1623963300
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Uber(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.Uber, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        // GET reports/uber/1622494799
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Uber(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.Uber, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        // GET: reports/uber/
        [HttpGet]
        public IActionResult Uber()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.Uber, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion
    }
}