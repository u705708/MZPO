using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Generic amoCRM repository, provides access to all main entities.
    /// </summary>
    /// <typeparam name="T">Entity in amoCRM, must implement <see cref="IEntity"/></typeparam>
    public class GenericAmoRepository<T> : AbstractAmoRepository, IAmoRepo<T> where T : IEntity, new()
    {
        #region Definition
        private readonly IAmoAuthProvider _auth;
        private readonly string _entityLink;
        private readonly string _apiAddress;

        /// <summary>
        /// Generic amoCRM repository constructor.
        /// </summary>
        ///<param name = "acc" ><see cref="AmoAccount"/> object, must implement <see cref="IAmoAccount"/>.</param>
        public GenericAmoRepository(IAmoAccount acc)
        {
            _auth = acc.auth;
            _entityLink = (string)typeof(T).GetProperty("entityLink").GetValue(null, null);   //IModel.entityLink;
            _apiAddress = $"https://{acc.subdomain}.amocrm.ru/api/v4/";
        }
        #endregion

        #region Supplementary methods
        public IEnumerable<T> GetEmbedded(EntityList entity)
        {
            if ((entity is not null) && (entity._embedded is not null))
                return (List<T>)entity.GetType().GetNestedType("Embedded").GetField(_entityLink).GetValue(entity._embedded);
            return new List<T>();
        }

        private static O GetResult<O>(AmoRequest request, O o)
        {
            try 
            {
                var response = request.GetResponse();
                if (response == "") return o;
                JsonConvert.PopulateObject(WebUtility.UrlDecode(response), o); 
            }
            catch (Exception e) { throw new Exception("Unable to process response : " + e.Message); }
            return o;
        }

        private EntityList GetList(string uri)
        {
            EntityList entityList = new() { _links = new Dictionary<string, EntityList.Links>() };
            entityList._links.Add("next", new EntityList.Links() { href = uri });

            while (entityList._links.ContainsKey("next"))
            {
                AmoRequest request = new("GET", entityList._links["next"].href, _auth);
                string response;

                var next = entityList._links["next"].href;
                try { response = request.GetResponse(); }
                catch { response = ""; }
                
                if (response == "") break;
                
                try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), entityList); }
                catch { break; }
                
                if (entityList._links.ContainsKey("next") && next == entityList._links["next"].href) break;
            }

            return entityList;
        }

        private int GetCatalogId()
        {
            int acc_id = _auth.GetAccountId();
            if (acc_id == 19453687) return 5111;
            if (acc_id == 28395871) return 12463;
            throw new Exception($"No catalog_d for account {acc_id}");
        }
        #endregion

        #region Realization
        public IEnumerable<T> AddNew(IEnumerable<T> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("POST", uri, content, _auth);
            EntityList result = new();
            return GetEmbedded(GetResult(request, result));
        }
        public IEnumerable<T> AddNew(T payload) => AddNew(new List<T>() { payload });
        public IEnumerable<int> AddNewComplex(IEnumerable<T> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}/complex";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("POST", uri, content, _auth);
            List<ComplexResponse> result = new();
            return GetResult(request, result).Select(x => x.Id);
        }
        public IEnumerable<int> AddNewComplex(T payload) => AddNewComplex(new List<T>() { payload });
        public IEnumerable<T> GetByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}{_entityLink}?{criteria}";

            return GetEmbedded(GetList(uri));
        }
        public IEnumerable<T> BulkGetById(IEnumerable<int> ids)
        {
            List<T> result = new();

            if (!ids.Any()) return result;
            
            int i = 0;
            StringBuilder criteria = new("");

            foreach (var id in ids.Distinct())
            {
                criteria.Append($"filter[id][{i++}]={id}&");
                if (i % 10 == 0)
                {
                    criteria.Append($"with=companies,contacts,leads,catalog_elements");
                    result.AddRange(GetByCriteria(criteria.ToString()));
                    criteria = new("");
                    i = 0;
                }
            }

            if (criteria.ToString() != "")
            {
                criteria.Append($"with=companies,contacts,leads,catalog_elements");
                result.AddRange(GetByCriteria(criteria.ToString()));
            }

            return result;
        }
        public T GetById(int id)
        {
            var uri = $"{_apiAddress}{_entityLink}/{id}?with=leads,contacts,companies,catalog_elements";

            AmoRequest request = new("GET", uri, _auth);
            return GetResult(request, new T());
        }
        public IEnumerable<T> Save(IEnumerable<T> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("PATCH", uri, content, _auth);
            EntityList result = new();
            return GetEmbedded(GetResult(request, result));
        }
        public IEnumerable<T> Save(T payload) => Save(new List<T>() { payload });
        public IEnumerable<Event> GetEntityEvents(int id)
        {
            var uri = $"{_apiAddress}events?filter[entity]={_entityLink[0..^1]}&filter[entity_id][]={id}";

            var result = GetList(uri);

            if (result._embedded is not null && result._embedded.events is not null)
                return result._embedded.events.ToList();
            return new List<Event>();
        }
        public IEnumerable<Event> GetEventsByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}events?{criteria}";

            var result = GetList(uri);

            if (result._embedded is not null && result._embedded.events is not null)
                return result._embedded.events.ToList();
            return new List<Event>();
        }
        public IEnumerable<Note> GetEntityNotes(int id)
        {
            var uri = $"{_apiAddress}{_entityLink}/{id}/notes";

            var result = GetList(uri);

            if (result._embedded is not null && result._embedded.notes is not null)
                return result._embedded.notes.ToList();
            return new List<Note>();
        }
        public IEnumerable<Note> GetNotesByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}{_entityLink}/notes?{criteria}";

            var result = GetList(uri);

            if (result._embedded is not null && result._embedded.notes is not null)
                return result._embedded.notes.ToList();
            return new List<Note>();
        }
        public Note GetNoteById(int id)
        {
            var uri = $"{_apiAddress}{_entityLink}/notes/{id}";

            AmoRequest request = new("GET", uri, _auth);
            return GetResult(request, new Note());
        }
        public IEnumerable<Note> BulkGetNotesById(IEnumerable<int> ids)
        {
            List<Note> result = new();

            if (!ids.Any()) return result;

            int i = 0;
            StringBuilder criteria = new("");

            foreach (var id in ids.Distinct())
            {
                criteria.Append($"filter[id][{i++}]={id}&");
                if (i % 10 == 0)
                {
                    result.AddRange(GetNotesByCriteria(criteria.ToString()));
                    criteria = new("");
                    i = 0;
                }
            }

            if (criteria.ToString() != "")
                result.AddRange(GetNotesByCriteria(criteria.ToString()));

            return result;
        }
        public IEnumerable<Note> AddNotes(IEnumerable<Note> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}/notes";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("POST", uri, content, _auth);
            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.notes is not null)
                return result._embedded.notes.ToList();
            return new List<Note>();
        }
        public IEnumerable<Note> AddNotes(Note note) => AddNotes(new List<Note>() { note });
        public IEnumerable<Note> AddNotes(int id, string comment) => AddNotes(new Note() { 
            entity_id = id, 
            note_type = "service_message", 
            parameters = new Note.Params() 
            { 
                service = "mzpo2amo", 
                text = comment 
            }});
        public IEnumerable<Tag> GetTags()
        {
            var uri = $"{_apiAddress}{_entityLink}/tags";

            var result = GetList(uri);

            if (result._embedded is not null && result._embedded.tags is not null)
                return result._embedded.tags.ToList();
            return new List<Tag>();
        }
        public IEnumerable<Tag> AddTag(IEnumerable<Tag> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}/tags";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("POST", uri, content, _auth);
            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.tags is not null)
                return result._embedded.tags.ToList();
            return new List<Tag>();
        }
        public IEnumerable<Tag> AddTag(Tag newTag) => AddTag(new List<Tag>() { newTag });
        public IEnumerable<Tag> AddTag(string tagName) => AddTag(new Tag() { name = tagName });
        public IEnumerable<CustomField> GetFields()
        {
            var uri = $"{_apiAddress}{_entityLink}/custom_fields";

            var result = GetList(uri);

            if (result._embedded is not null && result._embedded.custom_fields is not null)
                return result._embedded.custom_fields.ToList();
            return new List<CustomField>();
        }
        public IEnumerable<CustomField> AddField(IEnumerable<CustomField> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}/custom_fields";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("POST", uri, content, _auth);
            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.custom_fields is not null)
                return result._embedded.custom_fields.ToList();
            return new List<CustomField>();
        }
        public IEnumerable<CustomField> AddField(CustomField customField) => AddField(new List<CustomField>() { customField });
        public IEnumerable<CustomField> AddField(string fieldName) => AddField(new CustomField() { name = fieldName, type = "text" });
        public CatalogElement GetCEById(int id)
        {
            var catalog_id = 12463;
            var uri = $"{_apiAddress[0..^3]}v2/catalog_elements?catalog_id={catalog_id}&id={id}";

            var result = GetList(uri);

            if (result._embedded is not null && result._embedded.items is not null)
                return result._embedded.items.First();
            return null;
        }
        public IEnumerable<CatalogElement> GetCEs()
        {
            var catalog_id = 12463;
            var uri = $"{_apiAddress[0..^3]}v2/catalog_elements?catalog_id={catalog_id}";

            var result = GetList(uri);

            if (result._embedded is not null && result._embedded.items is not null)
                return result._embedded.items.ToList();
            return new List<CatalogElement>();
        }
        public IEnumerable<CatalogElement> AddCEs(IEnumerable<CatalogElement> elements)
        {
            int catalog_id = GetCatalogId();
            foreach (var e in elements)
                e.catalog_id = catalog_id;
            var payload = new { add = elements };

            var uri = $"{_apiAddress[0..^3]}v2/catalog_elements";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            AmoRequest request = new("POST", uri, content, _auth);

            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.items is not null)
                return result._embedded.items.ToList();
            return new List<CatalogElement>();
        }
        public IEnumerable<CatalogElement> AddCEs(CatalogElement element) => AddCEs(new List<CatalogElement>() { element });
        public IEnumerable<CatalogElement> UpdateCEs(IEnumerable<CatalogElement> elements)
        {
            int catalog_id = GetCatalogId();
            foreach (var e in elements)
                e.catalog_id = catalog_id;
            var payload = new { update = elements };

            var uri = $"{_apiAddress[0..^3]}v2/catalog_elements";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            AmoRequest request = new("POST", uri, content, _auth);

            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.items is not null)
                return result._embedded.items.ToList();
            return new List<CatalogElement>();
        }
        public IEnumerable<CatalogElement> UpdateCEs(CatalogElement element) => UpdateCEs(new List<CatalogElement>() { element });
        public IEnumerable<CatalogElement> DeleteCEs(IEnumerable<CatalogElement> elements)
        {
            int catalog_id = GetCatalogId();
            foreach (var e in elements)
                e.catalog_id = catalog_id;
            var payload = new { delete = elements };

            var uri = $"{_apiAddress[0..^3]}v2/catalog_elements";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            AmoRequest request = new("POST", uri, content, _auth);

            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.items is not null)
                return result._embedded.items.ToList();
            return new List<CatalogElement>();
        }
        public IEnumerable<CatalogElement> DeleteCEs(CatalogElement element) => DeleteCEs(new List<CatalogElement>() { element });

        public void AcceptUnsorted(string uid)
        {
            var uri = $"{_apiAddress}leads/unsorted/{uid}/accept";
            AmoRequest request = new("POST", uri, _auth);
            try 
            { 
                request.GetResponse(); 
            }
            catch (Exception e) 
            { 
                throw new Exception($"Unable to accept unsorted: {e.Message}"); 
            }
        }
        #endregion
    }
}