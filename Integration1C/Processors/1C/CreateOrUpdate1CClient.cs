using MZPO.AmoRepo;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class CreateOrUpdate1CClient
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly int _leadId;
        private readonly int _amo_acc;

        public CreateOrUpdate1CClient(Amo amo, Log log, int leadId, int amo_acc)
        {
            _amo = amo;
            _log = log;
            _leadId = leadId;
            _amo_acc = amo_acc;
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

        private static void PopulateContactCFs(Client1C client1C, int acc_id, Contact contact)
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

        private static void PopulateClientCFs(Contact contact, int amo_acc, Client1C client1C)
        {
            if (contact.custom_fields_values is not null)
                foreach (var p in client1C.GetType().GetProperties())
                    if (FieldLists.Contacts[amo_acc].ContainsKey(p.Name) &&
                        contact.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[amo_acc][p.Name]))
                        p.SetValue(client1C, contact.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[amo_acc][p.Name]).values[0].value);
        }

        private static Client1C CreateClient(Contact contact, int amo_acc)
        {
            Client1C client1C = new() { name = contact.name };

            PopulateClientCFs(contact, amo_acc, client1C);

            return client1C;
        }

        private static void UpdateContactInAmo(Client1C client1C, IAmoRepo<Contact> contRepo, int contact_id, int acc_id)
        {
            Contact contact = new()
            {
                id = contact_id,
                name = client1C.name,
                custom_fields_values = new(),
            };

            PopulateContactCFs(client1C, acc_id, contact);

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

            PopulateContactCFs(client1C, acc_id, contact);

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

        private static void UpdateClientIn1C()
        {
            throw new NotImplementedException();
        }

        private static void CreateClientIn1C()
        {
            throw new NotImplementedException();
        }

        private static void UpdateAmoEntities()
        {
            throw new NotImplementedException();
        }


        public void Run()
        {
            Lead lead = _amo.GetAccountById(_amo_acc).GetRepo<Lead>().GetById(_leadId);

            if (lead._embedded is null ||
                lead._embedded.contacts is null ||
                !lead._embedded.contacts.Any())
                return;

            #region Getting contacts
            List<int> contactIds = lead._embedded.contacts.Select(x => (int)x.id).ToList();

            var contRepo = _amo.GetAccountById(_amo_acc).GetRepo<Contact>();

            var contacts = contRepo.BulkGetById(contactIds); 
            #endregion

            #region Check for UIDs
            foreach (var c in contacts)
                if (c.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]) &&
                    Guid.TryParse((string)c.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]).values[0].value, out Guid value))
                {
                    UpdateClientIn1C();
                    return;
                }
            #endregion

            #region Creating Client
            if (!contacts.Any(x => x.custom_fields_values is not null))
                return;

            Contact contact = contacts.First(x => x.custom_fields_values is not null);

            Client1C client1C = CreateClient(contact, _amo_acc); 
            #endregion

            foreach (var a in amo_accounts)
            {
                if (a == _amo_acc)
                {
                    client1C.amo_ids.Add(new() { 
                        account_id = _amo_acc,
                        entity_id = (int)contact.id
                    });
                    continue;
                }

                #region Checking contact
                List<Contact> similarContacts = new();
                if (client1C.phone is not null &&
                    client1C.phone != "")
                    similarContacts.AddRange(contRepo.GetByCriteria($"query={client1C.phone}"));

                if (client1C.email is not null &&
                    client1C.email != "")
                    similarContacts.AddRange(contRepo.GetByCriteria($"query={client1C.email}"));

                if (similarContacts.Distinct(new ContactsComparer()).Count() > 1)
                    _log.Add($"Check for doubles: {JsonConvert.SerializeObject(similarContacts.Distinct(new ContactsComparer()), Formatting.Indented)}");
                #endregion

                #region Check for UIDs
                foreach (var c in similarContacts)
                    if (c.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]) &&
                        Guid.TryParse((string)c.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]).values[0].value, out Guid value))
                    {
                        UpdateClientIn1C();
                        return;
                    } 
                #endregion

                #region Updating found contact
                if (similarContacts.Any())
                {
                    UpdateContactInAmo(client1C, contRepo, (int)similarContacts.First().id, a);
                    client1C.amo_ids.Add(new()
                    {
                        account_id = a,
                        entity_id = (int)similarContacts.First().id
                    });
                    continue;
                }
                #endregion

                #region Creating new contact
                var compId = CreateContactInAmo(client1C, contRepo, a);

                client1C.amo_ids.Add(new()
                {
                    account_id = a,
                    entity_id = compId
                });
                #endregion
            }

            CreateClientIn1C();

            UpdateAmoEntities();
        }
    }
}