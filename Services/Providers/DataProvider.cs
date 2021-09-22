using Microsoft.Extensions.DependencyInjection;
using MZPO.AmoRepo;
using MZPO.DBRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Services
{
    public class DataProvider
    {
        #region Definition
        private readonly IServiceScopeFactory scopeFactory;
        //private readonly List<Data.Tag> _tags;
        //private readonly List<CF> _cfs;

        public DataProvider(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }
        #endregion

        #region Supplementary methods
        public void UpdateTags(List<AmoRepo.Tag> amoTags)
        {

        }                                                               //Дописать

        public void UpdateCFs(List<CustomField> amoCFs)
        {

        }                                                                   //Дописать
        #endregion

        #region Cities
        public async Task<string> GetRusCityNameAsync(string engName)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICityRepo>();
            var cityPair = await db.GetCityByEngName(engName);
            if (cityPair is null)
                return engName;
            return cityPair.RusName;
        }

        public async Task<City> GetCityAsync(string engName)
        {
            var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICityRepo>();
            return await db.GetCityByEngName(engName);
        }

        public async Task AddNewCityAsync(string engCity, string rusCity)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICityRepo>();
            var city = await db.GetCityByEngName(engCity);
            if (city is null)
            {
                await db.AddCity(new City() { EngName = engCity, RusName = rusCity });
                return;
            }
            city.RusName = rusCity;
            await db.UpdateCity(city);
        }

        public async Task<List<City>> GetCityListAsync()
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICityRepo>();
            return await db.GetAllCities();
        }
        #endregion

        #region Tags
        public async Task<DBRepository.Tag> GetTagAsync(int tagId, AmoAccount account)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
            return await db.GetTagById(tagId, account.id);
        }

        public async Task<string> GetTagNameAsync(int tagId, AmoAccount account)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
            var tag = await db.GetTagById(tagId, account.id);
            return tag.Name;
        }

        public async Task<int> GetTagIdAsync(string tagName, AmoAccount account)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
            var tag = await db.GetTagByName(tagName, account.id);
            if (tag is not null)
                return tag.Id;
            return await AddNewTagAsync(tagName, account);
        }

        public async Task<int> AddNewTagAsync(string tagName, AmoAccount account)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
            var tag = await db.GetTagByName(tagName, account.id);
            if (tag is not null) 
                return tag.Id;
            try
            {
                var leadRepo = account.GetRepo<Lead>();
                var newTag = leadRepo.AddTag(tagName).FirstOrDefault();
                await db.AddTag(new DBRepository.Tag()
                                    {
                                        Id = newTag.id,
                                        Name = newTag.name,
                                        AmoId = account.id
                                    });
                return newTag.id;
            }
            catch (Exception e) { throw new InvalidOperationException($"Unable to add tag {tagName}: {e.Message}"); }
        }

        public async Task<List<AmoRepo.Tag>> GetTagListAsync(AmoAccount account)
        {
            List<AmoRepo.Tag> result = new();
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
            var tags = await db.GetAllTags();
            return tags.Select(x => new AmoRepo.Tag() { id = x.Id, name = x.Name }).ToList();
        }
        #endregion

        #region CustomFileds
        public async Task<CF> GetCFAsync(int fieldId, AmoAccount acc)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
            return await db.GetCFByIdAsync(fieldId, acc.id);
        }

        public async Task<string> GetCFNameAsync(int fieldId, AmoAccount acc)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
            var cf = await db.GetCFByIdAsync(fieldId, acc.id);
            return cf.Name;
        }

        public async Task<int> GetCFIdAsync(string fieldName, AmoAccount acc)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
            var cf = await db.GetCFByNameAsync(fieldName, acc.id);
            if (cf is not null)
                return cf.Id;
            return await AddNewCFAsync(fieldName, acc);
        }

        public async Task<int> AddNewCFAsync(string fieldName, AmoAccount acc)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
            var cf = await db.GetCFByNameAsync(fieldName, acc.id);
            if (cf is not null)
                return cf.Id;
            try
            {
                var leadRepo = acc.GetRepo<Lead>();
                var newCF = leadRepo.AddField(fieldName).FirstOrDefault();
                await db.AddCFAsync(new CF()
                                        {
                                            Id = newCF.id,
                                            Name = newCF.name,
                                            AmoId = acc.id
                                        });
                return newCF.id;
            }
            catch (Exception e) { throw new InvalidOperationException($"Unable to add custom field {fieldName}: {e.Message}"); }
        }

        public async Task<List<CustomField>> GetCFListAsync(AmoAccount acc)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
            var list = await db.GetAllCFsAsync();
            return list.Select(x => new CustomField() { id = x.Id, name = x.Name }).ToList();
        }
        #endregion
    }
}
