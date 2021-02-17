using System.Threading.Tasks;

namespace MZPO.ReportProcessors
{
    internal interface IReportProcessor
    {
        public Task Run();
    }
}
