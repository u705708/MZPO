using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        private static O GetResult<O>(AmoRequest request, O o)
        {
            try 
            {
                var response = request.GetResponseAsync().Result;
                if (response == "") return o;
                JsonConvert.PopulateObject(WebUtility.UrlDecode(response), o); 
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception e) { throw new ArgumentException("Unable to process response : " + e.Message); }
            return o;
        }

        #region Yield realization
        public IEnumerable<E> GetEmbedded<E>(EntityList entity, string entityName)
        {
            if ((entity is not null) && (entity._embedded is not null))
                return (List<E>)entity.GetType().GetNestedType("Embedded").GetField(entityName).GetValue(entity._embedded);
            return new List<E>();
        }

        private IEnumerable<E> GetEntities<E>(string uri, string entityName)
        {
            EntityList entityList = new() { _links = new Dictionary<string, EntityList.Links>() };
            entityList._links.Add("next", new EntityList.Links() { href = uri });

            while (entityList._links.ContainsKey("next"))
            {
                AmoRequest request = new("GET", entityList._links["next"].href, _auth);
                string response;

                var next = entityList._links["next"].href;
                try { response = request.GetResponseAsync().Result; }
                catch(AmoRequest.TooManyRequestsException e) { throw new InvalidOperationException(e.Message); }
                catch { break; }

                if (response == "") break;

                entityList._embedded = new();

                try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), entityList); }
                catch { break; }

                foreach (var e in GetEmbedded<E>(entityList, entityName))
                    yield return e;

                if (entityList._links.ContainsKey("next") && next == entityList._links["next"].href) break;
            }

            yield break;
        } 
        #endregion

        private int GetCatalogId()
        {
            int acc_id = _auth.GetAccountId();
            if (acc_id == 19453687) return 5111;
            if (acc_id == 28395871) return 12463;
            if (acc_id == 29490250) return 5835;
            throw new ArgumentException($"No catalog_id for account {acc_id}");
        }
        #endregion

        #region Realization
        public IEnumerable<T> AddNew(IEnumerable<T> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("POST", uri, content, _auth);
            EntityList result = new();
            return GetEmbedded<T>(GetResult(request, result), _entityLink);
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

            return GetEntities<T>(uri, _entityLink);
        }
        public IEnumerable<T> BulkGetById(IEnumerable<int> ids)
        {
            List<T> result = new();

            if (!ids.Any()) return result;
            
            int i = 0;
            StringBuilder criteria = new("");

            List<string> allCriteria = new();

            foreach (var id in ids.Distinct())
            {
                criteria.Append($"filter[id][{i++}]={id}&");
                if (i % 10 == 0)
                {
                    criteria.Append($"with=companies,contacts,leads,catalog_elements");
                    allCriteria.Add(criteria.ToString());
                    criteria = new("");
                    i = 0;
                }
            }

            if (criteria.ToString() != "")
            {
                criteria.Append($"with=companies,contacts,leads,catalog_elements");
                allCriteria.Add(criteria.ToString());
            }

            Parallel.ForEach(
                allCriteria,
                new ParallelOptions { MaxDegreeOfParallelism = 6 },
                c => {
                    var entities = GetByCriteria(c).ToList();
                    lock (result) result.AddRange(entities);
                } );

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
            return GetEmbedded<T>(GetResult(request, result), _entityLink);
        }
        public IEnumerable<T> Save(T payload) => Save(new List<T>() { payload });

        public IEnumerable<Event> GetEntityEvents(int id)
        {
            string entityType = _entityLink switch {
                "leads" => "lead",
                "contacts" => "contact",
                "companies" => "company",
                _ => throw new InvalidOperationException($"No events for entity {_entityLink}"),
            };
            var uri = $"{_apiAddress}events?filter[entity]={entityType}&filter[entity_id][]={id}";

            return GetEntities<Event>(uri, "events");
        }
        public IEnumerable<Event> GetEventsByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}events?{criteria}";

            return GetEntities<Event>(uri, "events");
        }

        public IEnumerable<AmoTask> GetEntityTasks(int id)
        {
            var uri = $"{_apiAddress}tasks?filter[entity_type]={_entityLink}&filter[entity_id][]={id}";

            return GetEntities<AmoTask>(uri, "tasks");
        }
        public IEnumerable<AmoTask> GetTasksByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}tasks?{criteria}";

            return GetEntities<AmoTask>(uri, "tasks");
        }
        public AmoTask GetTaskById(int id)
        {
            var uri = $"{_apiAddress}tasks/{id}";

            AmoRequest request = new("GET", uri, _auth);
            return GetResult(request, new AmoTask());
        }
        public IEnumerable<AmoTask> BulkGetTasksById(IEnumerable<int> ids)
        {
            List<AmoTask> result = new();

            if (!ids.Any()) return result;

            int i = 0;
            StringBuilder criteria = new("");

            List<string> allCriteria = new();

            foreach (var id in ids.Distinct())
            {
                criteria.Append($"filter[id][{i++}]={id}&");
                if (i % 10 == 0)
                {
                    allCriteria.Add(criteria.ToString());
                    criteria = new("");
                    i = 0;
                }
            }

            if (criteria.ToString() != "")
                allCriteria.Add(criteria.ToString());

            Parallel.ForEach(
                allCriteria,
                new ParallelOptions { MaxDegreeOfParallelism = 6 },
                c => {
                    var tasks = GetTasksByCriteria(c).ToList();
                    lock (result) result.AddRange(tasks);
                });

            return result;
        }
        public IEnumerable<AmoTask> AddTasks(IEnumerable<AmoTask> payload)
        {
            var uri = $"{_apiAddress}tasks";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("POST", uri, content, _auth);
            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.tasks is not null)
                return result._embedded.tasks.ToList();
            return new List<AmoTask>();
        }
        public IEnumerable<AmoTask> AddTasks(AmoTask task) => AddTasks(new List<AmoTask>() { task });
        public IEnumerable<AmoTask> AddTasks(int entity_id, string text, long complete_till) => AddTasks(new AmoTask()
        {
            entity_id = entity_id,
            text = text,
            entity_type = _entityLink,
            complete_till = complete_till
        });
        public IEnumerable<AmoTask> EditTasks(IEnumerable<AmoTask> payload)
        {
            var uri = $"{_apiAddress}tasks";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("PATCH", uri, content, _auth);
            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.tasks is not null)
                return result._embedded.tasks.ToList();
            return new List<AmoTask>();
        }
        public IEnumerable<AmoTask> EditTasks(AmoTask task) => EditTasks(new List<AmoTask>() { task });
        public IEnumerable<AmoTask> CompleteTask(int id, string result_text) => EditTasks(new AmoTask()
        {
            id = id,
            is_completed = true,
            result = new AmoTask.Result()
            {
                text = result_text
            }
        });

        public IEnumerable<Note> GetEntityNotes(int id)
        {
            var uri = $"{_apiAddress}{_entityLink}/{id}/notes";

            return GetEntities<Note>(uri, "notes");
        }
        public IEnumerable<Note> GetNotesByCriteria(string criteria)
        {
            var uri = $"{_apiAddress}{_entityLink}/notes?{criteria}";

            return GetEntities<Note>(uri, "notes");
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

            List<string> allCriteria = new();

            foreach (var id in ids.Distinct())
            {
                criteria.Append($"filter[id][{i++}]={id}&");
                if (i % 10 == 0)
                {
                    allCriteria.Add(criteria.ToString());
                    criteria = new("");
                    i = 0;
                }
            }

            if (criteria.ToString() != "")
                allCriteria.Add(criteria.ToString());

            Parallel.ForEach(
                allCriteria,
                new ParallelOptions { MaxDegreeOfParallelism = 6 },
                c => {
                    var notes = GetNotesByCriteria(c).ToList();
                    lock (result) result.AddRange(notes);
                } );

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
        public IEnumerable<Note> AddNotes(int id, string comment) => AddNotes(new Note()
        {
            entity_id = id,
            note_type = "common",
            parameters = new Note.Params()
            {
                text = comment
            }
        });
        public IEnumerable<Note> AddServiceNotes(int id, string comment) => AddNotes(new Note() { 
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

            return GetEntities<Tag>(uri, "tags");
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

            return GetEntities<CustomField>(uri, "custom_fields");
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
            int catalog_id = GetCatalogId();
            var uri = $"{_apiAddress[0..^3]}v2/catalog_elements?catalog_id={catalog_id}&id={id}";

            var result = GetEntities<CatalogElement>(uri, "items");

            if (result.Any())
                return result.First();

            return null;
        }
        public IEnumerable<CatalogElement> GetCEs()
        {
            int catalog_id = GetCatalogId();
            int i = 1;

            List<CatalogElement> result = new();

            while (true)
            {
                int j = 0;
                var uri = $"{_apiAddress[0..^3]}v2/catalog_elements?catalog_id={catalog_id}&page={i++}";
                var batch = GetEntities<CatalogElement>(uri, "items");
                foreach (var ce in batch)
                {
                    yield return ce;
                    j++;
                }

                if (j < 50) yield break;
            }
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

        public IEnumerable<EntityLink> LinkEntity(int entity_id, IEnumerable<EntityLink> payload)
        {
            var uri = $"{_apiAddress}{_entityLink}/{entity_id}/link";

            var content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            AmoRequest request = new("POST", uri, content, _auth);
            EntityList list = new();
            var result = GetResult(request, list);
            if (result._embedded is not null && result._embedded.links is not null)
                return result._embedded.links.ToList();
            return new List<EntityLink>();
        }
        public IEnumerable<EntityLink> LinkEntity(int entity_id, EntityLink entityLink) => LinkEntity(entity_id, new List<EntityLink>() { entityLink });
        public IEnumerable<EntityLink> LinkEntity(int entity_id, int to_entity_id, string to_entity_type) => LinkEntity(entity_id, new EntityLink() { to_entity_id = to_entity_id, to_entity_type = to_entity_type });

        public async void AcceptUnsorted(string uid)
        {
            var uri = $"{_apiAddress}leads/unsorted/{uid}/accept";
            AmoRequest request = new("POST", uri, _auth);
            try 
            { 
                await request.GetResponseAsync(); 
            }
            catch (Exception e) 
            { 
                throw new InvalidOperationException($"Unable to accept unsorted: {e.Message}"); 
            }
        }
        #endregion
    }
}