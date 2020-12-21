using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    public interface IBaseRepo<T> where T: IModel, new()
    {
        public IEnumerable<T> AddNew(IEnumerable<T> payload);
        public IEnumerable<T> GetByCriteria(string criteria);
        public T GetById(int id);
        public IEnumerable<T> Save(IEnumerable<T> payload);
        public IEnumerable<T> Save(T payload);
        public IEnumerable<Note> GetNotes(int id);
        public IEnumerable<Note> AddNotes(int id, string comment);
        public IEnumerable<Note> AddNotes(Note note);
        public IEnumerable<Note> AddNotes(IEnumerable<Note> payload);
        public IEnumerable<Tag> GetTags();
        public IEnumerable<Tag> AddTag(IEnumerable<Tag> payload);
        public IEnumerable<Tag> AddTag(Tag newTag);
        public IEnumerable<Tag> AddTag(string tagName);
        public IEnumerable<CustomField> GetFields();
        public IEnumerable<CustomField> AddField(IEnumerable<CustomField> payload);
        public IEnumerable<CustomField> AddField(CustomField customField);
        public IEnumerable<CustomField> AddField(string fieldName);
        public void AcceptUnsorted(string uid);
    }
}
