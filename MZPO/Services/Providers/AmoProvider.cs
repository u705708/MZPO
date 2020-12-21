using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MZPO.Data;

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
        public List<AmoAccountAuth> GetAmoAccounts() 
        {
            List<AmoAccountAuth> result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
                result = db.GetAllAccounts().Result;
            }
            return result;
        }

        public AmoAccountAuth GetAmoAccount(int accountId)
        {
            AmoAccountAuth result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
                result = db.GetAmoAccountById(accountId).Result;
            }
            return result;
        }
        
        public async void AddAccount(AmoAccountAuth account)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
                var authAccounts = db.GetAllAccounts().Result;
                if (authAccounts.Any(x => x.id == account.id)) return;
                await db.AddAmoAccount(account);
            }
        }

        public async void RemoveAccount(AmoAccountAuth account)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
                await db.RemoveAmoAccount(db.GetAmoAccountById(account.id).Result);
            }
        }

        public async void UpdateAccount(AmoAccountAuth account)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IAccountRepo>();
                var oldAccount = db.GetAmoAccountById(account.id).Result;
                if (oldAccount == null) AddAccount(account);
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
        }
        #endregion
    }
}