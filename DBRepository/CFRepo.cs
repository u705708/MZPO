using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.DBRepository
{
    public class CFRepo : ICFRepo
    {
        private readonly MySQLContext db;

        public CFRepo(MySQLContext context)
        {
            db = context;
        }

        #region CFs
        public async Task<List<CF>> GetAllCFsAsync()
        {
            return await db.CFs.ToListAsync();
        }

        public async Task<CF> GetCFByIdAsync(int id, int amoId)
        {
            return await db.CFs.FirstOrDefaultAsync(x => (x.Id == id) && (x.AmoId == amoId));
        }

        public async Task<CF> GetCFByNameAsync(string name, int amoId)
        {
            return await db.CFs.FirstOrDefaultAsync(x => (x.Name == name) && (x.AmoId == amoId));
        }

        public async Task<int> AddCFAsync(CF cf)
        {
            db.CFs.Add(cf);
            return await db.SaveChangesAsync();
        }

        public async Task<int> RemoveCFAsync(CF cf)
        {
            db.CFs.Remove(cf);
            return await db.SaveChangesAsync();
        }

        public async Task<int> UpdateCFAsync(CF cf)
        {
            db.CFs.Update(cf);
            return await db.SaveChangesAsync();
        }
        #endregion
    }
}