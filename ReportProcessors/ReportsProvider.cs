using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.ReportProcessors

{
    public enum Reports
    {
        CorporateSales,
        KPI,
        LongLeads,
        UnfinishedCompanies,
        WeeklyReport
    }

    public static class ReportsProvider
    {
        private static readonly Dictionary<Reports, ReportParams> ReportParameters = new()
        {
            { Reports.CorporateSales, new("1jzqcptdlCpSPXcyLpumSGCaHtSVi28bg8Ga2aEFXCoQ", "CorporateSales", "report_corp", 19453687) },
            { Reports.KPI, new("1ZjdabzAtTQKKdK5ZtGfvYT2jA-JN6agO0QMxtWPed0k", "KPI", "report_kpi", 28395871) },
            { Reports.LongLeads, new("1EtpEiq5meigVrY9-n3phHxQRVO3iHgpF6V0-wpJ5Yg4", "LongLeads", "report_long", 28395871) },
            { Reports.UnfinishedCompanies, new("1JTAzCS89hLxI9fA3MWxiE9BSzZro3nPhyfy8931rZTk", "UnfinishedCompanies", "report_corp_unfinished", 19453687) },
            { Reports.WeeklyReport, new("1HDZALGgRt_HsLyNf45_R52ngo1ggvy02IxiOLGg02hA", "WeeklyReport", "report_retail", 28395871) }
        };

        //private static readonly Dictionary<Reports, ReportParams> ReportParameters = new()
        //{
        //    { Reports.CorporateSales, new("1NuP1qpKDuWlQAje0mIA4i73KgfTH6TGi5iLvzMY46pU", "CorporateSales", "report_corp", 19453687) },
        //    { Reports.KPI, new("1NuP1qpKDuWlQAje0mIA4i73KgfTH6TGi5iLvzMY46pU", "KPI", "report_kpi", 28395871) },
        //    { Reports.LongLeads, new("1NuP1qpKDuWlQAje0mIA4i73KgfTH6TGi5iLvzMY46pU", "LongLeads", "report_long", 28395871) },
        //    { Reports.UnfinishedCompanies, new("1NuP1qpKDuWlQAje0mIA4i73KgfTH6TGi5iLvzMY46pU", "UnfinishedCompanies", "report_corp_unfinished", 19453687) },
        //    { Reports.WeeklyReport, new("1NuP1qpKDuWlQAje0mIA4i73KgfTH6TGi5iLvzMY46pU", "WeeklyReport", "report_retail", 28395871) }
        //};

        private static IReportProcessor ReportFactory(ReportParams reportParams, AmoAccount acc, TaskList processQueue, GSheets gSheets, long dateFrom, long dateTo, CancellationToken token)
        {
            return reportParams.ReportName switch
            {
                "CorporateSales" => new CorpReportProcessor(acc, processQueue, gSheets, reportParams.SheetId, dateFrom, dateTo, reportParams.TaskName, token),
                "KPI" => new RetailKPIProcessor(acc, processQueue, gSheets, reportParams.SheetId, dateFrom, dateTo, reportParams.TaskName, token),
                "LongLeads" => new LongLeadsProcessor(acc, processQueue, gSheets, reportParams.SheetId, dateFrom, dateTo, reportParams.TaskName, token),
                "UnfinishedCompanies" => new UnfinishedContactsProcessor(acc, processQueue, gSheets, reportParams.SheetId, dateFrom, dateTo, reportParams.TaskName, token),
                "WeeklyReport" => new WeeklyKPIReportProcessor(acc, processQueue, gSheets, reportParams.SheetId, dateFrom, dateTo, reportParams.TaskName, token),
                _ => throw new Exception($"Unknown report type: {reportParams.ReportName}"),
            };
        }

        public static void StartReport(Reports rep, Amo amo, TaskList processQueue, GSheets gSheets, long dateFrom, long dateTo)
        {
            var reportParams = ReportParameters[rep];
            var acc = amo.GetAccountById(reportParams.AmoAccount);

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Lazy<IReportProcessor> reportProcessor = new(() => ReportFactory(reportParams, acc, processQueue, gSheets, dateFrom, dateTo, token));

            Task task = Task.Run(() => reportProcessor.Value.Run());
            processQueue.AddTask(task, cts, reportParams.TaskName, acc.name, reportParams.ReportName);
        }
    }
}