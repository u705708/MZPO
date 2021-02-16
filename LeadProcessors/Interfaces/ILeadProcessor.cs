using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public interface ILeadProcessor
    {
        public Task Run();
    }
}
