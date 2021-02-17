using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.DBRepository
{
    public class TagRepo : ITagRepo
    {
        private readonly MySQLContext db;

        public TagRepo(MySQLContext context)
        {
            db = context;
        }

        #region Tags
        public async Task<List<Tag>> GetAllTags()
        {
            return await db.Tags.ToListAsync();
        }

        public async Task<Tag> GetTagById(int id, int amoId)
        {
            return await db.Tags.FirstOrDefaultAsync(x => (x.Id == id) && (x.AmoId==amoId));
        }

        public async Task<Tag> GetTagByName(string name, int amoId)
        {
            return await db.Tags.FirstOrDefaultAsync(x => (x.Name == name) && (x.AmoId == amoId));
        }

        public async Task<int> AddTag(Tag tag)
        {
            db.Tags.Add(tag);
            return await db.SaveChangesAsync();
        }

        public async Task<int> RemoveTag(Tag tag)
        {
            db.Tags.Remove(tag);
            return await db.SaveChangesAsync();
        }

        public async Task<int> UpdateTag(Tag tag)
        {
            db.Tags.Update(tag);
            return await db.SaveChangesAsync();
        }
        #endregion
    }
}
