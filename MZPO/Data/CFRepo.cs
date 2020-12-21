using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.Data
{
    public class CFRepo : ICFRepo
    {
        private MySQLContext db;

        public CFRepo(MySQLContext context)
        {
            db = context;
        }

        #region CFs
        public async Task<List<CF>> GetAllCFs()
        {
            return await db.CFs.ToListAsync();
        }

        public async Task<CF> GetCFById(int id, int amoId)
        {
            return await db.CFs.FirstOrDefaultAsync(x => (x.Id == id) && (x.AmoId == amoId));
        }

        public async Task<CF> GetCFByName(string name, int amoId)
        {
            return await db.CFs.FirstOrDefaultAsync(x => (x.Name == name) && (x.AmoId == amoId));
        }

        public async Task<int> AddCF(CF cf)
        {
            db.CFs.Add(cf);
            return await db.SaveChangesAsync();
        }

        public async Task<int> RemoveCF(CF cf)
        {
            db.CFs.Remove(cf);
            return await db.SaveChangesAsync();
        }

        public async Task<int> UpdateCF(CF cf)
        {
            db.CFs.Update(cf);
            return await db.SaveChangesAsync();
        }
        #endregion
    }
}
