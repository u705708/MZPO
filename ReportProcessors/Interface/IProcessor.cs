using System.Threading.Tasks;

namespace MZPO.ReportProcessors
{
    public interface IProcessor
    {
        public Task Run();
    }
}
