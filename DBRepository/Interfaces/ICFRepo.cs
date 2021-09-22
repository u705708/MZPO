using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.DBRepository
{
    public interface ICFRepo
    {
        public Task<List<CF>> GetAllCFsAsync();
        public Task<CF> GetCFByIdAsync(int id, int amoId);
        public Task<CF> GetCFByNameAsync(string name, int amoId);
        public Task<int> AddCFAsync(CF cf);
        public Task<int> RemoveCFAsync(CF cf);
        public Task<int> UpdateCFAsync(CF cf);
    }
}
