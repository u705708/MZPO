using Microsoft.Extensions.DependencyInjection;
using MZPO.AmoRepo;
using MZPO.Data;
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
        public string GetRusCityName(string engName)
        {
            string result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICityRepo>();
                result = db.GetCityByEngName(engName).Result.RusName;
            }
            return result;
        }

        public City GetCity(string engName)
        {
            City result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICityRepo>();
                result = db.GetCityByEngName(engName).Result;
            }
            return result;
        }

        public async void AddNewCity(string engCity, string rusCity)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICityRepo>();
                var city = db.GetCityByEngName(engCity).Result;
                if (city == null)
                {
                    await db.AddCity(new City() { EngName = engCity, RusName = rusCity });
                }
                else
                {
                    city.RusName = rusCity;
                    await db.UpdateCity(city);
                }
            }
        }

        public List<City> GetCityList()
        {
            List<City> result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICityRepo>();
                result = db.GetAllCities().Result;
            }
            return result;
        }
        #endregion

        #region Tags
        public Data.Tag GetTag(int tagId, AmoAccount account)
        {
            Data.Tag result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
                result = db.GetTagById(tagId, account.id).Result;
            }
            return result;
        }

        public string GetTagName(int tagId, AmoAccount account)
        {
            string result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
                result = db.GetTagById(tagId, account.id).Result.Name;
            }
            return result;
        }

        public int GetTagId(string tagName, AmoAccount account)
        {
            int result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
                var tag = db.GetTagByName(tagName, account.id).Result;
                if (tag != null) result = tag.Id;
                else result = AddNewTag(tagName, account).Result;
            }
            return result;
        }

        public async Task<int> AddNewTag(string tagName, AmoAccount account)
        {
            int result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
                var tag = db.GetTagByName(tagName, account.id).Result;
                if (tag != null) result = tag.Id;
                else
                {
                    try
                    {
                        var leadRepo = account.GetRepo<Lead>();
                        var newTag = leadRepo.AddTag(tagName).FirstOrDefault();
                        result = newTag.id;
                        await db.AddTag(new Data.Tag()
                        {
                            Id = newTag.id,
                            Name = newTag.name,
                            AmoId = account.id
                        });
                    }
                    catch (Exception e) { throw new Exception($"Unable to add tag {tagName}: {e.Message}"); }
                }
            }
            return result;
        }

        public List<AmoRepo.Tag> GetTagList(AmoAccount account)
        {
            List<AmoRepo.Tag> result = new List<AmoRepo.Tag>(); 
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ITagRepo>();
                foreach (var t in db.GetAllTags().Result)
                {
                    if (t.AmoId == account.id)
                        result.Add(new AmoRepo.Tag()
                        {
                            id = t.Id,
                            name = t.Name
                        });
                }
            }
            return result;
        }
        #endregion

        #region CustomFileds
        public CF GetCF(int fieldId, AmoAccount acc)
        {
            CF result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
                result = db.GetCFById(fieldId, acc.id).Result;
            }
            return result;
        }

        public string GetCFName(int fieldId, AmoAccount acc)
        {
            string result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
                result = db.GetCFById(fieldId, acc.id).Result.Name;
            }
            return result;
        }

        public int GetCFId(string fieldName, AmoAccount acc)
        {
            int result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
                var cf = db.GetCFByName(fieldName, acc.id);
                if (cf != null) result = cf.Id;
                else result = AddNewCF(fieldName, acc).Result;
            }
            return result;

        }

        public async Task<int> AddNewCF(string fieldName, AmoAccount acc)
        {
            int result;
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
                var cf = db.GetCFByName(fieldName, acc.id).Result;
                if (cf != null) result = cf.Id;
                else
                {
                    try
                    {
                        var leadRepo = acc.GetRepo<Lead>();
                        var newCF = leadRepo.AddField(fieldName).FirstOrDefault();
                        result = newCF.id;
                        await db.AddCF(new CF()
                        {
                            Id = newCF.id,
                            Name = newCF.name,
                            AmoId = acc.id
                        });
                    }
                    catch (Exception e) { throw new Exception($"Unable to add custom field {fieldName}: {e.Message}"); }
                }
            }
            return result;
        }

        public List<CustomField> GetCFList(AmoAccount acc)
        {
            List<CustomField> result = new List<CustomField>();
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ICFRepo>();
                foreach (var c in db.GetAllCFs().Result)
                {
                    if (c.AmoId == acc.id)
                        result.Add(new CustomField()
                        {
                            id = c.Id,
                            name = c.Name
                        });
                }
            }
            return result;
        }
        #endregion
    }
}
