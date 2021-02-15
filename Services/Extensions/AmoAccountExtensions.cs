using MZPO.AmoRepo;
using MZPO.Data;
using System.Collections.Generic;
using System.Linq;

namespace MZPO.Services
{
    public static class AmoAccountExtensions
    {
        public static GenericAmoRepository<T> GetRepo<T>(this IAmoAccount acc) where T : IEntity, new()
        {
            return new GenericAmoRepository<T>(acc);
        }

        #region City
        public static string GetCity(this AmoAccount acc, string input)
        {
            return acc.dataProvider.GetRusCityName(input);
        }

        public static void AddNewCity(this AmoAccount acc, string engCity, string rusCity)
        {
            acc.dataProvider.AddNewCity(engCity, rusCity);
        }

        public static List<City> GetCityList(this AmoAccount acc)
        {
            return acc.dataProvider.GetCityList();
        }
        #endregion

        #region Tags
        public static int GetTagId(this AmoAccount acc, string tagName)
        {
            return acc.dataProvider.GetTagId(tagName, acc);
        }

        public static int AddNewTag(this AmoAccount acc, string tagName)
        {
            return acc.dataProvider.AddNewTag(tagName, acc).Result;
        }

        public static Dictionary<string, int> GetTagDictionary(this AmoAccount acc)
        {
            var result = new Dictionary<string, int>();
            var list = acc.dataProvider.GetTagList(acc);
            if (list.Any())
                foreach (var l in list)
                    result.Add(l.name, l.id);
            return result;
        }
        #endregion

        #region Custom fields
        public static int GetCFId(this AmoAccount acc, string fieldName)
        {
            return acc.dataProvider.GetCFId(fieldName, acc);
        }

        public static int AddNewCF(this AmoAccount acc, string cfName)
        {
            return acc.dataProvider.AddNewCF(cfName, acc).Result;
        }

        public static Dictionary<string, int> GetCFDictionary(this AmoAccount acc)
        {
            var result = new Dictionary<string, int>();
            var list = acc.dataProvider.GetCFList(acc);
            if (list.Any())
                foreach (var l in list)
                    result.Add(l.name, l.id);
            return result;
        }
        #endregion
    }
}
