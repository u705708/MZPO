using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;

namespace Integration1C
{
    public class UpdateAmoContact
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Client1C _client1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public UpdateAmoContact(Client1C client1C, Amo amo, Log log, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _client1C = client1C;
            _filter = filter;
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
                    if (p.Name == "dob")
                    {
                        DateTime dob = (DateTime)p.GetValue(client1C);
                        value = ((DateTimeOffset)dob.AddHours(3)).ToUnixTimeSeconds();
                    }

                    try { if ((string)value == "") continue; }
                    catch { }

                    if (contact.custom_fields_values is null) contact.custom_fields_values = new();

                    contact.custom_fields_values.Add(new Custom_fields_value()
                    {
                        field_id = FieldLists.Contacts[acc_id][p.Name],
                        values = new Custom_fields_value.Values[] { new Custom_fields_value.Values() { value = value } }
                    });
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
                throw new Exception($"Unable to update contact {contact_id} in amo: {e.Message}");
            }
        }

        public List<Amo_id> Run()
        {
            try
            {
                if (_client1C.amo_ids is not null)
                    foreach (var c in _client1C.amo_ids)
                    {
                        UpdateContactInAmo(_client1C, _amo.GetAccountById(c.account_id).GetRepo<Contact>(), c.entity_id, c.account_id, _filter);
                        
                        _log.Add($"Updated contact {c.entity_id} in amo {c.account_id}.");
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