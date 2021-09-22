using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MZPO.DBRepository;

namespace MZPO.Services
{
    public class AmoProvider
    {
        #region Definition
        private readonly IServiceScopeFactory scopeFactory;

        public AmoProvider(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }
        #endregion

        #region Realization
        public async Task<List<AmoAccountAuth>> GetAllAmoAccountsAsync() 
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
            return await db.GetAllAccounts();
        }

        public async Task<AmoAccountAuth> GetAmoAccountAsync(int accountId)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
            return await db.GetAmoAccountById(accountId);
        }
        
        public async Task AddAccountAsync(AmoAccountAuth account)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
            var authAccounts = await db.GetAllAccounts();
            if (authAccounts.Any(x => x.id == account.id)) return;
            await db.AddAmoAccount(account);
        }

        public async Task RemoveAccountAsync(AmoAccountAuth account)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
            await db.RemoveAmoAccount(await db.GetAmoAccountById(account.id));
        }

        public async Task UpdateAccountAsync(AmoAccountAuth account)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
            var oldAccount = await db.GetAmoAccountById(account.id);
            if (oldAccount is null) await AddAccountAsync(account);
            else
            {
                oldAccount.name = account.name;
                oldAccount.subdomain = account.subdomain;
                oldAccount.client_id = account.client_id;
                oldAccount.client_secret = account.client_secret;
                oldAccount.redirect_uri = account.redirect_uri;
                oldAccount.code = account.code;
                oldAccount.authToken = account.authToken;
                oldAccount.refrToken = account.refrToken;
                oldAccount.validity = account.validity;
                await db.UpdateAmoAccount(oldAccount);
            }
        }
        #endregion
    }
}