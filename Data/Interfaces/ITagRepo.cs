using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.DBRepository
{
    public interface ITagRepo
    {
        public Task<List<Tag>> GetAllTags();
        public Task<Tag> GetTagById(int id, int amoId);
        public Task<Tag> GetTagByName(string name, int amoId);
        public Task<int> AddTag(Tag tag);
        public Task<int> RemoveTag(Tag tag);
        public Task<int> UpdateTag(Tag tag);
    }
}
