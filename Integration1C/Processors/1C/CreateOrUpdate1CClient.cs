using MZPO.AmoRepo;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class CreateOrUpdate1CClient
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly int _leadId;
        private readonly int _amo_acc;
        private readonly ClientRepository _repo1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public CreateOrUpdate1CClient(Amo amo, Log log, int leadId, int amo_acc, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _leadId = leadId;
            _amo_acc = amo_acc;
            _repo1C = new(cred1C);
            _filter = filter;
        }

        private readonly List<int> amo_accounts = new()
        {
            19453687,
            28395871,
            //29490250
        };

        class ContactsComparer : IEqualityComparer<Contact>
        {
            public bool Equals(Contact x, Contact y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (x is null || y is null)
                    return false;

                return x.id == y.id;
            }

            public int GetHashCode(Contact c)
            {
                if (c is null) return 0;

                int hashProductCode = c.id.GetHashCode();

                return hashProductCode;
            }
        }

        private static void PopulateContactCFs(Client1C client1C, int acc_id, Contact contact)
        {
            foreach (var p in client1C.GetType().GetProperties())
                if (FieldLists.Contacts[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(client1C) is not null)
                {
                    var value = p.GetValue(client1C);
                    if (p.Name == "dob" ||
                        p.Name == "pass_issued_at")
                    {
                        DateTime dt = (DateTime)p.GetValue(client1C);
                        value = ((DateTimeOffset)dt.AddHours(3)).ToUnixTimeSeconds();
                    }

                    try { if ((string)value == "") continue; }
                    catch { }

                    if (contact.custom_fields_values is null) contact.custom_fields_values = new();

                    contact.custom_fields_values.Add(new Custom_fields_value()
                    {
                        field_id = FieldLists.Contacts[acc_id][p.Name],
                        values = new Custom_fields_value.Value[] { new Custom_fields_value.Value() { value = value } }
                    });
                }
        }

        private static void PopulateClientCFs(Contact contact, int amo_acc, Client1C client1C)
        {
            if (contact.custom_fields_values is not null)
                foreach (var p in client1C.GetType().GetProperties())
                    if (FieldLists.Contacts[amo_acc].ContainsKey(p.Name) &&
                        contact.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[amo_acc][p.Name]))
                    {
                        var value = contact.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[amo_acc][p.Name]).values[0].value;
                        if (p.PropertyType == typeof(Guid?) &&
                            Guid.TryParse((string)value, out Guid guidValue))
                        {
                            p.SetValue(client1C, guidValue);
                            continue;
                        }

                        if (p.PropertyType == typeof(DateTime?) ||
                            p.PropertyType == typeof(DateTime))
                        {
                            if ((long)value < -2208996153) value = -2208996153;
                            var dt = DateTimeOffset.FromUnixTimeSeconds((long)value).UtcDateTime.AddDays(1);
                            p.SetValue(client1C, new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0 ));
                            continue;
                        }

                        p.SetValue(client1C, value);
                    }
        }

        private static void UpdateAmoId(Contact contact, Client1C client1C)
        {
            if (client1C.amo_ids is null)
                client1C.amo_ids = new();

            if (!client1C.amo_ids.Any(x => x.account_id == contact.account_id))
            {
                client1C.amo_ids.Add(new() { account_id = (int)contact.account_id, entity_id = (int)contact.id });
                return;
            }

            if (!client1C.amo_ids.Any(x => x.entity_id == contact.id))
            {
                client1C.amo_ids.First(x => x.account_id == contact.account_id).entity_id = (int)contact.id;
                return;
            }
        }

        private static Client1C CreateClient(Contact contact, int amo_acc)
        {
            Client1C client1C = new() { name = contact.name };

            PopulateClientCFs(contact, amo_acc, client1C);

            return client1C;
        }

        private static void UpdateContactInAmo(Client1C client1C, IAmoRepo<Contact> contRepo, int contact_id, int acc_id, RecentlyUpdatedEntityFilter filter)
        {
            Contact contact = new()
            {
                id = contact_id,
                name = client1C.name,
            };

            PopulateContactCFs(client1C, acc_id, contact);

            try
            {
                filter.AddEntity(contact_id);
                contRepo.Save(contact);
            }
            catch (Exception e)
            {
                throw new Exception($"1C ERROR: Unable to update contact {contact_id} in amo: {e}");
            }
        }

        private static int CreateContactInAmo(Client1C client1C, IAmoRepo<Contact> contRepo, int acc_id, RecentlyUpdatedEntityFilter filter)
        {
            Contact contact = new()
            {
                name = client1C.name,
            };

            PopulateContactCFs(client1C, acc_id, contact);

            try
            {
                var result = contRepo.AddNew(contact).ToList();
                result.ForEach(x => filter.AddEntity((int)x.id));
                if (result.Any())
                    return (int)result.First().id;
                else throw new Exception("Amo returned no contact Ids.");
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to create contact in amo: {e}");
            }
        }

        private static void UpdateClientIn1C(Guid client_id_1C, Contact contact, int amo_acc, Amo amo, Log log, ClientRepository repo1C)
        {
            var client1C = repo1C.GetClient(client_id_1C);
            if (client1C == default) throw new Exception($"1C ERROR: Unable to add client to 1C. 1C returned default client_id.");

            PopulateClientCFs(contact, amo_acc, client1C);

            client1C.name = contact.name;

            UpdateAmoId(contact, client1C);

            repo1C.UpdateClient(client1C);
        }

        private static void CreateClientIn1C(Client1C client1C, ClientRepository repo1C)
        {
            var result = repo1C.AddClient(client1C);
            if (result == default) throw new Exception("1C ERROR: Unable to add client to 1C. 1C returned default client_id.");
            client1C.client_id_1C = result;
        }

        private static void UpdateAmoEntities(IAmoRepo<Contact> contRepo, Amo_id amo_id, Guid uid, RecentlyUpdatedEntityFilter filter)
        {
            Contact contact = new() {
                id = amo_id.entity_id,
                custom_fields_values = new() { new Custom_fields_value() {
                        field_id = FieldLists.Contacts[amo_id.account_id]["client_id_1C"],
                        values = new Custom_fields_value.Value[] { new Custom_fields_value.Value() { value = uid.ToString("D") } }
            } } };

            try
            {
                filter.AddEntity(amo_id.entity_id);
                contRepo.Save(contact);
            }
            catch (Exception e)
            {
                throw new Exception($"amo ERROR: Unable to update contact {amo_id.entity_id} in amo {amo_id.account_id}: {e.Message}");
            }
        }

        private static void AddLeadNote(IAmoRepo<Lead> repo, int leadNumber, string text)
        {
            repo.AddNotes(new Note() { entity_id = leadNumber, note_type = "common", parameters = new Note.Params() { text = text } });
        }

        public Guid Run()
        {
            try
            {
                var leadRepo = _amo.GetAccountById(_amo_acc).GetRepo<Lead>();

                Lead lead = leadRepo.GetById(_leadId);

                if (lead._embedded is null ||
                    lead._embedded.contacts is null ||
                    !lead._embedded.contacts.Any())
                    throw new Exception($"Not a suitable lead {_leadId} for creating contact in 1C: no lead or contacts.");

                #region Getting contacts
                List<int> contactIds = lead._embedded.contacts.Select(x => (int)x.id).ToList();

                var contRepo = _amo.GetAccountById(_amo_acc).GetRepo<Contact>();

                var contacts = contRepo.BulkGetById(contactIds).ToList();

                if (!contacts.Any(x => x.custom_fields_values is not null))
                    throw new Exception($"No suitable contacts to add to 1C at lead {_leadId}");
                #endregion

                #region Check for UIDs
                foreach (var c in contacts)
                    if (c.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]) &&
                        Guid.TryParse((string)c.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]).values[0].value, out Guid client_id_1C))
                    {
                        UpdateClientIn1C(client_id_1C, c, _amo_acc, _amo, _log, _repo1C);
                        
                        _log.Add($"Updated client {c.id} in 1C {client_id_1C}.");
                        
                        return client_id_1C;
                    }
                #endregion

                if (contacts.Count > 1)
                {
                    AddLeadNote(leadRepo, _leadId, "Ошибка переноса в 1С: в сделке больше одного контакта");
                    throw new Exception($"Several suitable contacts to add to 1C at lead {_leadId}");
                }

                #region Creating Client
                Contact contact = contacts.First(x => x.custom_fields_values is not null);

                Client1C client1C = CreateClient(contact, _amo_acc);
                #endregion

                foreach (var a in amo_accounts)
                {
                    if (client1C.amo_ids is null) client1C.amo_ids = new();
                    if (a == _amo_acc)
                    {
                        client1C.amo_ids.Add(new()
                        {
                            account_id = _amo_acc,
                            entity_id = (int)contact.id
                        });
                        continue;
                    }

                    var anotherContRepo = _amo.GetAccountById(a).GetRepo<Contact>();

                    #region Checking contact
                    List<Contact> similarContacts = new();
                    if (!string.IsNullOrEmpty(client1C.phone))
                        similarContacts.AddRange(anotherContRepo.GetByCriteria($"query={client1C.phone}"));

                    if (!string.IsNullOrEmpty(client1C.email))
                        similarContacts.AddRange(anotherContRepo.GetByCriteria($"query={client1C.email}"));

                    if (similarContacts.Distinct(new ContactsComparer()).Count() > 1)
                        _log.Add($"Check for doubles: {JsonConvert.SerializeObject(similarContacts.Distinct(new ContactsComparer()).Select(x => new { x.id, x.account_id }), Formatting.Indented)}");
                    #endregion

                    #region Check for UIDs
                    foreach (var c in similarContacts)
                        if (c.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[a]["client_id_1C"]) &&
                            Guid.TryParse((string)c.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[a]["client_id_1C"]).values[0].value, out Guid client_id_1C))
                        {
                            UpdateClientIn1C(client_id_1C, contact, a, _amo, _log, _repo1C);

                            _log.Add($"Found and updated client {c.id} in 1C {client_id_1C}.");

                            return client_id_1C;
                        }
                    #endregion

                    #region Adding found contact
                    if (similarContacts.Any())
                    {
                        UpdateContactInAmo(client1C, anotherContRepo, (int)similarContacts.First().id, a, _filter);
                        client1C.amo_ids.Add(new()
                        {
                            account_id = a,
                            entity_id = (int)similarContacts.First().id
                        });

                        _log.Add($"Found and updated client {similarContacts.First().id} in 1C {client1C.client_id_1C}.");
                        
                        continue;
                    }
                    #endregion

                    #region Creating new contact
                    var compId = CreateContactInAmo(client1C, anotherContRepo, a, _filter);

                    client1C.amo_ids.Add(new()
                    {
                        account_id = a,
                        entity_id = compId
                    });

                    _log.Add($"Created new client {compId} in 1C {client1C.client_id_1C}.");
                    #endregion
                }

                CreateClientIn1C(client1C, _repo1C);

                foreach (var a in amo_accounts)
                    UpdateAmoEntities(_amo.GetAccountById(a).GetRepo<Contact>(), client1C.amo_ids.First(x => x.account_id == a), (Guid)client1C.client_id_1C, _filter);

                return (Guid)client1C.client_id_1C;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to create or update client in 1C from lead {_leadId}: {e.Message}");
                return default;
            }
        }
    }
}