using Microsoft.AspNetCore.Mvc;
using MZPO.ReportProcessors;
using MZPO.Services;
using System;

namespace MZPO.Controllers.ReportProcessors
{
    [Route("[controller]/[action]")]
    public class ReportsController : Controller
    {
        private readonly ProcessQueue _processQueue;
        private readonly Amo _amo;
        private readonly GSheets _gSheets;

        public ReportsController(Amo amo, ProcessQueue processQueue, GSheets gSheets)
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

        #region Without dates
        // GET: reports/unfinishedcompanies
        [ActionName("UnfinishedCompanies")]
        [HttpGet]
        public IActionResult UnfinishedCompanies()
        {
            ReportsProvider.StartReport(Reports.UnfinishedCompanies, _amo, _processQueue, _gSheets, 0, 0);

            return Ok("Requested.");
        }

        // GET: reports/abandonedcompanies
        [ActionName("AbandonedCompanies")]
        [HttpGet]
        public IActionResult AbandonedCompanies()
        {
            ReportsProvider.StartReport(Reports.AbandonedCompanies, _amo, _processQueue, _gSheets, 0, 0);

            return Ok("Requested.");
        }

        // GET: reports/companieslastcontacts
        [ActionName("CompaniesLastContacts")]
        [HttpGet]
        public IActionResult CompaniesLastContacts()
        {
            ReportsProvider.StartReport(Reports.CompaniesLastContacts, _amo, _processQueue, _gSheets, 0, 0);

            return Ok("Requested.");
        }
        #endregion

        #region CorporateSales
        // GET reports/corporatesales/1630443600,1633035599
        [ActionName("CorporateSales")]
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult CorporateSales(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.CorporateSales, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/corporatesales/1619816399
        [ActionName("CorporateSales")]
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult CorporateSales(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.CorporateSales, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/corporatesales
        [ActionName("CorporateSales")]
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
        [ActionName("KPI")]
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult KPI(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/kpi/1619816399
        [ActionName("KPI")]
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult KPI(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates();

            ReportsProvider.StartReport(Reports.KPI, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/kpi
        [ActionName("KPI")]
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
        [ActionName("LongLeads")]
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult LongLeads(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/longleads/1619816399
        [ActionName("LongLeads")]
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult LongLeads(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.LongLeads, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/longleads
        [ActionName("LongLeads")]
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
        [ActionName("WeeklyReport")]
        [HttpGet("{from},{to}")]                                                                                                                       //Запрашиваем отчёт для диапазона дат
        public IActionResult WeeklyReport(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/weeklyreport/1619816399
        [ActionName("WeeklyReport")]
        [HttpGet("{to}")]                                                                                                                       //Запрашиваем отчёт для диапазона дат
        public IActionResult WeeklyReport(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.WeeklyReport, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/weeklyreport/
        [ActionName("WeeklyReport")]
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
        [ActionName("MonthlyReport")]
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult MonthlyReport(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.KPI_monthly, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/monthlyreport/1619816399
        [ActionName("MonthlyReport")]
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult MonthlyReport(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.KPI_monthly, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/monthlyreport/
        [ActionName("MonthlyReport")]
        [HttpGet]
        public IActionResult MonthlyReport()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.KPI_monthly, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region DoublesList
        // GET reports/doubleslist/1625086799,1633017599
        [ActionName("DoublesList")]
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult DoublesList(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.Doubles, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/doubleslist/1619816399
        [ActionName("DoublesList")]
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult DoublesList(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.Doubles, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/doubleslist/
        [ActionName("DoublesList")]
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
        [ActionName("Cards")]
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Cards(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.Cards, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/cards/1622494799
        [ActionName("Cards")]
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Cards(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.Cards, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/cards/
        [ActionName("Cards")]
        [HttpGet]
        public IActionResult Cards()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.Cards, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region Uber
        // GET reports/uber/1598907600,1630928600
        [ActionName("Uber")]
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Uber(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.Uber, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/uber/1622494799
        [ActionName("Uber")]
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Uber(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.Uber, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/uber/
        [ActionName("Uber")]
        [HttpGet]
        public IActionResult Uber()
        {
            var dates = GetDates();

            ReportsProvider.StartReport(Reports.Uber, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }
        #endregion

        #region Calls
        // GET reports/calls/1598907600,1625086799
        [ActionName("Calls")]
        [HttpGet("{from},{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Calls(string from, string to)
        {
            if (!long.TryParse(from, out long dateFrom) &
                !long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            ReportsProvider.StartReport(Reports.SuccessCalls, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }

        // GET reports/calls/1622494799
        [ActionName("Calls")]
        [HttpGet("{to}")]                                                                                                                //Запрашиваем отчёт для диапазона дат
        public IActionResult Calls(string to)
        {
            if (!long.TryParse(to, out long dateTo)) return BadRequest("Incorrect dates");

            var dates = GetDates(dateTo);

            ReportsProvider.StartReport(Reports.SuccessCalls, _amo, _processQueue, _gSheets, dates.Item1, dates.Item2);

            return Ok("Requested.");
        }

        // GET: reports/calls/
        [ActionName("Calls")]
        [HttpGet]
        public IActionResult Calls()
        {
            var now = DateTime.UtcNow.AddHours(3);
            var to = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(-3).AddSeconds(-1);
            var from = to.AddSeconds(1).AddDays(-1);
            long dateFrom = ((DateTimeOffset)from).ToUnixTimeSeconds();
            long dateTo = ((DateTimeOffset)to).ToUnixTimeSeconds();

            ReportsProvider.StartReport(Reports.SuccessCalls, _amo, _processQueue, _gSheets, dateFrom, dateTo);

            return Ok("Requested.");
        }
        #endregion
    }
}