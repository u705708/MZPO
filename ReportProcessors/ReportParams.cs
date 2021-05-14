namespace MZPO.ReportProcessors
{
    public class ReportParams
    {
        public string SheetId { get; private set; }
        public string ReportName { get; private set; }
        public string TaskId { get; private set; }
        public int AmoAccount { get; private set; }


        public ReportParams(string sheetId, string reportName, string taskId, int amoAccount)
        {
            SheetId = sheetId;
            ReportName = reportName;
            TaskId = taskId;
            AmoAccount = amoAccount;
        }
    }
}