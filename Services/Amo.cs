using Microsoft.Extensions.DependencyInjection;
using MZPO.DBRepository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MZPO.Services
{
    public class Amo
    {
        #region Definition
        private readonly IList<AmoAccount> _accounts;

        private readonly DataProvider dataProvider;
        private readonly AmoProvider amoProvider;

        public Amo(IServiceScopeFactory scopeFactory)
        {
            amoProvider = new AmoProvider(scopeFactory);
            dataProvider = new DataProvider(scopeFactory);

            var authAccounts = amoProvider.GetAmoAccounts();

            _accounts = new List<AmoAccount>();
            if (authAccounts.Any())
            {
                foreach (var acc in authAccounts)
                {
                    _accounts.Add(new AmoAccount()
                    {
                        id = acc.id,
                        name = acc.name,
                        subdomain = acc.subdomain,
                        auth = new AuthProvider(acc, amoProvider),
                        dataProvider = dataProvider
                    });
                }
            }
        }
        #endregion

        #region Realization
        public AmoAccount GetAccountById(int id)
        {
            if (_accounts.Any(x => x.id == id)) return _accounts.SingleOrDefault(x => x.id == id);
            else throw new Exception($"Error: No such amo account: {id}");
        }

        public AmoAccount GetAccountByName(string name)
        {
            if (_accounts.Any(x => x.name == name)) return _accounts.SingleOrDefault(x => x.name == name);
            else throw new Exception($"Error: No such amo account: {name}");
        }

        public void AddAmoAccount(int id, string name, string subdomain, string client_id, string client_secret, string redirect_uri, string code)
        {
            var acc = amoProvider.GetAmoAccount(id);
            if (acc is not null)
            {
                acc.name = name;
                acc.subdomain = subdomain;
                acc.client_id = client_id;
                acc.client_secret = client_secret;
                acc.redirect_uri = redirect_uri;
                acc.code = code;
                amoProvider.UpdateAccount(acc);
            }
            else
            {
                acc = new AmoAccountAuth()
                {
                    name = name,
                    subdomain = subdomain,
                    client_id = client_id,
                    client_secret = client_secret,
                    redirect_uri = redirect_uri,
                    code = code
                };
                amoProvider.AddAccount(acc);
            }
            if (_accounts.Any(x => x.id == id))
            {
                _accounts.First(x => x.id == id).name = name;
                _accounts.First(x => x.id == id).subdomain = subdomain;
                _accounts.First(x => x.id == id).auth = new AuthProvider(acc, amoProvider);
            }
            else
            {
                _accounts.Add(new AmoAccount()
                {
                    id = id,
                    name = name,
                    subdomain = subdomain,
                    auth = new AuthProvider(acc, amoProvider),
                    dataProvider = dataProvider
                });
            }
        }

        public void AddAmoAccount(int id, string name, string subdomain, string client_id, string client_secret, string redirect_uri, string code, string authToken, string refrToken, DateTime validity)
        {
            var acc = amoProvider.GetAmoAccount(id);
            if (acc is not null)
            {
                acc.name = name;
                acc.subdomain = subdomain;
                acc.client_id = client_id;
                acc.client_secret = client_secret;
                acc.redirect_uri = redirect_uri;
                acc.code = code;
                acc.authToken = authToken;
                acc.refrToken = refrToken;
                acc.validity = validity;
                amoProvider.UpdateAccount(acc);
            }
            else
            {
                acc = new AmoAccountAuth()
                {
                    id = id,
                    name = name,
                    subdomain = subdomain,
                    client_id = client_id,
                    client_secret = client_secret,
                    redirect_uri = redirect_uri,
                    code = code,
                    authToken = authToken,
                    refrToken = refrToken,
                    validity = validity
            };
                amoProvider.AddAccount(acc);
            }
            if (_accounts.Any(x => x.id == id))
            {
                _accounts.First(x => x.id == id).name = name;
                _accounts.First(x => x.id == id).subdomain = subdomain;
                _accounts.First(x => x.id == id).auth = new AuthProvider(acc, amoProvider);
            }
            else
            {
                _accounts.Add(new AmoAccount()
                {
                    id = id,
                    name = name,
                    subdomain = subdomain,
                    auth = new AuthProvider(acc, amoProvider),
                    dataProvider = dataProvider
                });
            }
        }

        public void RemoveAmoAccount(int id)
        {
            var acc = amoProvider.GetAmoAccount(id);
            if (acc is not null)
            {
                amoProvider.RemoveAccount(acc);
            }
            if (_accounts.Any(x => x.id == id))
            {
                _accounts.Remove(_accounts.First((x => x.id == id)));
            }
        }
        #endregion
    }
}