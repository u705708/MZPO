using MZPO.AmoRepo;
using MZPO.DBRepository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Services
{
    public static class AmoAccountExtensions
    {
        public static GenericAmoRepository<T> GetRepo<T>(this IAmoAccount acc) where T : IEntity, new()
        {
            return new GenericAmoRepository<T>(acc);
        }

        #region City
        public static async Task<string> GetCityAsync(this AmoAccount acc, string input)
        {
            return await acc.dataProvider.GetRusCityNameAsync(input);
        }

        public static async Task AddNewCityAsync(this AmoAccount acc, string engCity, string rusCity)
        {
            await acc.dataProvider.AddNewCityAsync(engCity, rusCity);
        }

        public static async Task<List<City>> GetCityListAsync(this AmoAccount acc)
        {
            return await acc.dataProvider.GetCityListAsync();
        }
        #endregion

        #region Tags
        public static async Task<int> GetTagIdAsync(this AmoAccount acc, string tagName)
        {
            return await acc.dataProvider.GetTagIdAsync(tagName, acc);
        }

        public static async Task<int> AddNewTagAsync(this AmoAccount acc, string tagName)
        {
            return await acc.dataProvider.AddNewTagAsync(tagName, acc);
        }

        public static async Task<Dictionary<string, int>> GetTagDictionaryAsync(this AmoAccount acc)
        {
            var list = await acc.dataProvider.GetTagListAsync(acc);
            return list.ToDictionary(x => x.name, x => x.id);
        }
        #endregion

        #region Custom fields
        public static async Task<int> GetCFIdAsync(this AmoAccount acc, string fieldName)
        {
            return await acc.dataProvider.GetCFIdAsync(fieldName, acc);
        }

        public static async Task<int> AddNewCFAsync(this AmoAccount acc, string cfName)
        {
            return await acc.dataProvider.AddNewCFAsync(cfName, acc);
        }

        public static async Task<Dictionary<string, int>> GetCFDictionaryAsync(this AmoAccount acc)
        {
            var list = await acc.dataProvider.GetCFListAsync(acc);
            return list.ToDictionary(x => x.name, x => x.id);
        }
        #endregion
    }
}