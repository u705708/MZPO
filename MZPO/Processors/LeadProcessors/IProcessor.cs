using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    interface IProcessor
    {
        public Task Run();
    }
}
