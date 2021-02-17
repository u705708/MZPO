using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.DBRepository
{
    public class AccountRepo : IAccountRepo
    {
        private readonly MySQLContext db;

        public AccountRepo(MySQLContext context)
        {
            db = context;
        }

        #region AmoAccounts
        public async Task<List<AmoAccountAuth>> GetAllAccounts()
        {
            return await db.AmoAccounts.ToListAsync();
        }

        public async Task<AmoAccountAuth> GetAmoAccountById(int id)
        {
            return await db.AmoAccounts.FirstOrDefaultAsync(x => x.id == id);
        }

        public async Task<AmoAccountAuth> GetAmoAccountByName(string name)
        {
            return await db.AmoAccounts.FirstOrDefaultAsync(x => x.name == name);
        }

        public async Task<int> AddAmoAccount(AmoAccountAuth amoAccount)
        {
            db.AmoAccounts.Add(amoAccount);
            return await db.SaveChangesAsync();
        }
        
        public async Task<int> RemoveAmoAccount(AmoAccountAuth amoAccount)
        {
            db.AmoAccounts.Remove(amoAccount);
            return await db.SaveChangesAsync();
        }

        public async Task<int> UpdateAmoAccount(AmoAccountAuth amoAccount)
        {
            db.AmoAccounts.Update(amoAccount);
            return await db.SaveChangesAsync();
        }
        #endregion
    }
}
