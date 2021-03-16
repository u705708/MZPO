﻿using MZPO.AmoRepo;
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

        public CreateOrUpdateAmoContact(Client1C client1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _client1C = client1C;
        }

        List<int> amo_accounts = new()
        {
            19453687,
            28395871
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
            contact.custom_fields_values.Add(new Contact.Custom_fields_value()
            {
                field_id = FieldLists.Contacts[acc_id]["company_id_1C"],
                values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = client1C.client_id_1C.ToString("D") } }
            });
        }

        private static void PopulateCFs(Client1C client1C, int acc_id, Contact contact)
        {
            foreach (var p in client1C.GetType().GetProperties())
                if (FieldLists.Contacts[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(client1C) is not null &&
                    (string)p.GetValue(client1C) != "") //В зависимости от политики передачи пустых полей
                {
                    contact.custom_fields_values.Add(new Contact.Custom_fields_value()
                    {
                        field_id = FieldLists.Contacts[acc_id][p.Name],
                        values = new Contact.Custom_fields_value.Values[] { new Contact.Custom_fields_value.Values() { value = (string)p.GetValue(client1C) } }
                    });
                }
        }

        private static void UpdateContactInAmo(Client1C client1C, IAmoRepo<Contact> contRepo, int contact_id, int acc_id)
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
                contRepo.Save(contact);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update contact {contact_id} in amo: {e}");
            }
        }

        private static int CreateContactInAmo(Client1C client1C, IAmoRepo<Contact> contRepo, int acc_id)
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
                var result = contRepo.AddNewComplex(contact);
                if (result.Any())
                    return result.First();
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
                    {
                        UpdateContactInAmo(_client1C, contRepo, _client1C.amo_ids.First(x => x.account_id == a).entity_id, a);
                        continue;
                    } 
                    #endregion

                    #region Checking contact
                    List<Contact> similarContacts = new();
                    if (_client1C.phone is not null &&
                        _client1C.phone != "")
                        similarContacts.AddRange(contRepo.GetByCriteria($"query={_client1C.phone}"));

                    if (_client1C.email is not null &&
                        _client1C.email != "")
                        similarContacts.AddRange(contRepo.GetByCriteria($"query={_client1C.email}"));

                    if (similarContacts.Distinct(new ContactsComparer()).Count() > 1)
                        _log.Add($"Check for doubles: {JsonConvert.SerializeObject(similarContacts.Distinct(new ContactsComparer()), Formatting.Indented)}");
                    #endregion

                    #region Updating found contact
                    if (similarContacts.Any())
                    {
                        UpdateContactInAmo(_client1C, contRepo, (int)similarContacts.First().id, a);
                        _client1C.amo_ids.Add(new()
                        {
                            account_id = a,
                            entity_id = (int)similarContacts.First().id
                        });
                        continue;
                    }
                    #endregion

                    #region Creating new contact
                    var compId = CreateContactInAmo(_client1C, contRepo, a);

                    _client1C.amo_ids.Add(new()
                    {
                        account_id = a,
                        entity_id = compId
                    });
                    #endregion
                }
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update company in amo from 1C: {e}");
            }

            return _client1C.amo_ids;
        }
    }
}