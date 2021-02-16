using System.Threading.Tasks;

namespace MZPO.ReportProcessors
{
    public interface IReportProcessor
    {
        public Task Run();
    }
}
