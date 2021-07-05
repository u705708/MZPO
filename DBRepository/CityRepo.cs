using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.DBRepository
{
    public class CityRepo : ICityRepo
    {
        private readonly MySQLContext db;

        public CityRepo(MySQLContext context)
        {
            db = context;
        }

        #region Cities
        public async Task<List<City>> GetAllCities()
        {
            return await db.Cities.ToListAsync();
        }

        public async Task<City> GetCityByEngName(string engName)
        {
            return await db.Cities.FirstOrDefaultAsync(x => x.EngName == engName);
        }

        public async Task<City> GetCityByRusName(string rusName)
        {
            return await db.Cities.FirstOrDefaultAsync(x => x.RusName == rusName);
        }

        public async Task<int> AddCity(City city)
        {
            db.Cities.Add(city);
            return await db.SaveChangesAsync();
        }

        public async Task<int> RemoveCity(City city)
        {
            db.Cities.Remove(city);
            return await db.SaveChangesAsync();
        }

        public async Task<int> UpdateCity(City city)
        {
            db.Cities.Update(city);
            return await db.SaveChangesAsync();
        }
        #endregion
    }
}