using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    public abstract class AbstractAmoRepository
    {
        public class EntityList
        {
            public int? _page;
            public Dictionary<string, Links> _links;
            public Embedded _embedded;

            public class Links
            {
                public string href;
            }

            public class Embedded
            {
                public IEnumerable<Lead> leads;
                public IEnumerable<Contact> contacts;
                //public IEnumerable<Customer> customers;
                public IEnumerable<Company> companies;
                public IEnumerable<Event> events;
                public IEnumerable<Note> notes;
                public IEnumerable<AmoTask> tasks;
                public IEnumerable<Tag> tags;
                public IEnumerable<CustomField> custom_fields;
                public IEnumerable<CatalogElement> items;
                public IEnumerable<EntityLink> links;
            }
        }
    }
}