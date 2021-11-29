using MZPO.AmoRepo;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class CreateOrUpdateAmoContact
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Client1C _client1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public CreateOrUpdateAmoContact(Client1C client1C, Amo amo, Log log, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _client1C = client1C;
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

        private static void AddUIDToEntity(Client1C client1C, int acc_id, Contact contact)
        {
            contact.custom_fields_values.Add(new Custom_fields_value()
            {
                field_id = FieldLists.Contacts[acc_id]["client_id_1C"],
                values = new Custom_fields_value.Values[] { new Custom_fields_value.Values() { value = client1C.client_id_1C.Value.ToString("D") } }
            });
        }

        private static void PopulateCFs(Client1C client1C, int acc_id, Contact contact)
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

                    contact.AddNewCF(FieldLists.Contacts[acc_id][p.Name], value);
                }
        }

        private static void UpdateContactInAmo(Client1C client1C, IAmoRepo<Contact> contRepo, int contact_id, int acc_id, RecentlyUpdatedEntityFilter filter)
        {
            Contact contact = new()
            {
                id = contact_id,
                name = client1C.name,
                custom_fields_values = new(),
            };

            AddUIDToEntity(client1C, acc_id, contact);

            PopulateCFs(client1C, acc_id, contact);

            try
            {
                filter.AddEntity(contact_id);
                contRepo.Save(contact);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update contact {contact_id} in amo: {e}");
            }
        }

        private static int CreateContactInAmo(Client1C client1C, IAmoRepo<Contact> contRepo, int acc_id, RecentlyUpdatedEntityFilter filter)
        {
            Contact contact = new()
            {
                name = client1C.name,
                custom_fields_values = new(),
            };

            AddUIDToEntity(client1C, acc_id, contact);

            PopulateCFs(client1C, acc_id, contact);

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

        public List<Amo_id> Run()
        {
            if (_client1C.amo_ids is null) _client1C.amo_ids = new();

            try
            {
                foreach (var a in amo_accounts)
                {
                    var contRepo = _amo.GetAccountById(a).GetRepo<Contact>();

                    #region Checking if contact already linked to entity and updating if possible
                    if (_client1C.amo_ids.Any(x => x.account_id == a))
                        try
                        {
                            UpdateContactInAmo(_client1C, contRepo, _client1C.amo_ids.First(x => x.account_id == a).entity_id, a, _filter);

                            _log.Add($"Updated contact {_client1C.amo_ids.First(x => x.account_id == a).entity_id} in amo {a}.");

                            continue;
                        } 
                        catch (Exception e)
                        {
                            _log.Add($"Unable to update existing contact {_client1C.amo_ids.First(x => x.account_id == a).entity_id} in amo. Trying to create new or find existing. {e}");
                        }
                    #endregion

                    #region Checking contact
                    List<Contact> similarContacts = new();
                    if (!string.IsNullOrEmpty(_client1C.phone))
                        similarContacts.AddRange(contRepo.GetByCriteria($"query={_client1C.phone}"));

                    if (!string.IsNullOrEmpty(_client1C.email))
                        similarContacts.AddRange(contRepo.GetByCriteria($"query={_client1C.email}"));

                    if (similarContacts.Distinct(new ContactsComparer()).Count() > 1)
                        _log.Add($"Check for doubles: {JsonConvert.SerializeObject(similarContacts.Distinct(new ContactsComparer()).Select(x => new { id = x.id, account_id = x.account_id }), Formatting.Indented)}");
                    #endregion

                    #region Updating found contact
                    if (similarContacts.Any())
                    {
                        UpdateContactInAmo(_client1C, contRepo, (int)similarContacts.First().id, a, _filter);
                        _client1C.amo_ids.Add(new()
                        {
                            account_id = a,
                            entity_id = (int)similarContacts.First().id
                        });

                        _log.Add($"Found and updated contact {similarContacts.First().id} in amo {a}.");

                        continue;
                    }
                    #endregion

                    #region Creating new contact
                    var compId = CreateContactInAmo(_client1C, contRepo, a, _filter);

                    _client1C.amo_ids.Add(new()
                    {
                        account_id = a,
                        entity_id = compId
                    });
                    #endregion

                    _log.Add($"Created contact {compId} in amo {a}.");
                }
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update contact in amo from 1C: {e.Message}");
            }

            return _client1C.amo_ids;
        }
    }
}