using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public interface IProcessor
    {
        public Task Run();
    }
}
