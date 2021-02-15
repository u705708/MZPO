using System.Threading.Tasks;

namespace MZPO.ReportProcessors
{
    interface IProcessor
    {
        public Task Run();
    }
}
