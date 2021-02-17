namespace MZPO.ReportProcessors
{
    public class ReportParams
    {
        public string SheetId { get; private set; }
        public string ReportName { get; private set; }
        public string TaskName { get; private set; }
        public int AmoAccount { get; private set; }


        public ReportParams(string sheetId, string reportName, string taskName, int amoAccount)
        {
            SheetId = sheetId;
            ReportName = reportName;
            TaskName = taskName;
            AmoAccount = amoAccount;
        }
    }
}