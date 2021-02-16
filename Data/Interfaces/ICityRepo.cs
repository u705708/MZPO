using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.Data
{
    public interface ICityRepo
    {
        public Task<List<City>> GetAllCities();
        public Task<City> GetCityByEngName(string engName);
        public Task<City> GetCityByRusName(string rusName);
        public Task<int> AddCity(City city);
        public Task<int> RemoveCity(City city);
        public Task<int> UpdateCity(City city);
    }
}
