using MZPO.Services;
using MZPO.AmoRepo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Integration1C
{
    class CreateOrUpdateAmoContact
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Client1C _client;
        private readonly string _request;
        private readonly IAmoRepo<Contact> _retRepo;
        private readonly IAmoRepo<Contact> _corpRepo;
        private readonly ClientRepository _1CRepo;

        public CreateOrUpdateAmoContact(string request, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _client = new();
            _request = request;
            _retRepo = _amo.GetAccountById(28395871).GetRepo<Contact>();
            _corpRepo = _amo.GetAccountById(19453687).GetRepo<Contact>();
            _1CRepo = new();
        }

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

        private static object SetFieldValue(Contact contact, int fieldId, object fieldValue)
        {
            if (contact.custom_fields_values is null) contact.custom_fields_values = new();

            if (contact.custom_fields_values.Any(x => x.field_id == fieldId))
                contact.custom_fields_values.First(x => x.field_id == fieldId).values[0].value = fieldValue;
            else
                contact.custom_fields_values.Add(new Contact.Custom_fields_value()
                {
                    field_id = fieldId,
                    values = new Contact.Custom_fields_value.Values[] {
                        new Contact.Custom_fields_value.Values() { value = fieldValue }
                    }
                });
            return fieldValue;
        }

        private static void UpdateContactInAmo(IAmoRepo<Contact> repo, Dictionary<string, int> fieldList, Client1C client, int id)
        {
            if (id == 0)
            {
                CreateContactInAmo(repo, fieldList, client);
                return;
            }

            Contact contact = new()
            {
                id = id,
                name = client.Name
            };

            SetFieldValue(contact, fieldList["client_id_1C"], client.Client_id_1C);
            SetFieldValue(contact, fieldList["phone"], client.Phone);
            SetFieldValue(contact, fieldList["email"], client.Email);
            SetFieldValue(contact, fieldList["dob"], (int)((DateTimeOffset)client.Dob).ToUnixTimeSeconds());
            SetFieldValue(contact, fieldList["pass_serie"], client.Pass_serie);
            SetFieldValue(contact, fieldList["pass_number"], client.Pass_number);
            SetFieldValue(contact, fieldList["pass_issued_by"], client.Pass_issued_by);
            SetFieldValue(contact, fieldList["pass_issued_at"], client.Pass_issued_at);
            SetFieldValue(contact, fieldList["pass_dpt_code"], client.Pass_dpt_code);

            try
            {
                var result = repo.Save(contact);

                if (result.Any())
                    switch (result.First().account_id)
                    {
                        case 19453687:
                            if (client.Amo_ids is null) client.Amo_ids = new();
                            //client.Amo_ids.corp_id = result.First().id;
                            return;
                        case 28395871:
                            if (client.Amo_ids is null) client.Amo_ids = new();
                            //client.Amo_ids.ret_id = result.First().id;
                            return;
                        default:
                            return;
                    }
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update contact in amo: {e}");
            }
        }

        private static void CreateContactInAmo(IAmoRepo<Contact> repo, Dictionary<string, int> fieldList, Client1C client)
        {
            Contact contact = new()
            {
                name = client.Name
            };

            SetFieldValue(contact, fieldList["client_id_1C"], client.Client_id_1C);
            SetFieldValue(contact, fieldList["phone"], client.Phone);
            SetFieldValue(contact, fieldList["email"], client.Email);
            SetFieldValue(contact, fieldList["dob"], (int)((DateTimeOffset)client.Dob).ToUnixTimeSeconds());
            SetFieldValue(contact, fieldList["pass_serie"], client.Pass_serie);
            SetFieldValue(contact, fieldList["pass_number"], client.Pass_number);
            SetFieldValue(contact, fieldList["pass_issued_by"], client.Pass_issued_by);
            SetFieldValue(contact, fieldList["pass_issued_at"], client.Pass_issued_at);
            SetFieldValue(contact, fieldList["pass_dpt_code"], client.Pass_dpt_code);

            try
            {
                var result = repo.AddNew(contact);

                if (result.Any())
                    switch (result.First().account_id)
                    {
                        case 19453687:
                            if (client.Amo_ids is null) client.Amo_ids = new();
                            //client.Amo_ids.corp_id = result.First().id;
                            return;
                        case 28395871:
                            if (client.Amo_ids is null) client.Amo_ids = new();
                            //client.Amo_ids.ret_id = result.First().id;
                            return;
                        default:
                            return;
                    }
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to create contact in amo: {e}");
            }
        }

        public void Run()
        {
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(_request), _client); }
            catch(Exception e) { _log.Add($"Unable to process request: {e} --- Request: {_request}"); return; }

            try
            {
                #region Checking if already exist
                if (_client.Amo_ids is not null)
                {
                    //UpdateContactInAmo(_retRepo, FieldLists.ContactRet, _client, _client.Amo_ids.ret_id);
                    //UpdateContactInAmo(_corpRepo, FieldLists.ContactCorp, _client, _client.Amo_ids.corp_id);
                    return;
                }
                #endregion

                #region Checking retail for contact
                List<Contact> retContacts = new();

                retContacts.AddRange(_retRepo.GetByCriteria($"query={_client.Client_id_1C}"));
                if (!retContacts.Any())
                {
                    retContacts.AddRange(_retRepo.GetByCriteria($"query={_client.Phone}"));
                    retContacts.AddRange(_retRepo.GetByCriteria($"query={_client.Email}"));
                }

                if (retContacts.Any() &&
                    retContacts.Distinct(new ContactsComparer()).Count() == 1)
                {
                    UpdateContactInAmo(_retRepo, FieldLists.ContactRet, _client, (int)retContacts.First().id);
                }
                else if (retContacts.Any() &&
                    retContacts.Distinct(new ContactsComparer()).Count() > 1)
                {
                    _log.Add($"Check for doubles: {JsonConvert.SerializeObject(retContacts.Distinct(new ContactsComparer()), Formatting.Indented)}");
                    CreateContactInAmo(_retRepo, FieldLists.ContactRet, _client);
                }
                else
                {
                    CreateContactInAmo(_retRepo, FieldLists.ContactRet, _client);
                }
                #endregion

                #region Checking corporate for contact
                List<Contact> corpContacts = new();

                corpContacts.AddRange(_corpRepo.GetByCriteria($"query={_client.Client_id_1C}"));
                if (!corpContacts.Any())
                {
                    corpContacts.AddRange(_corpRepo.GetByCriteria($"query={_client.Phone}"));
                    corpContacts.AddRange(_corpRepo.GetByCriteria($"query={_client.Email}"));
                }

                if (corpContacts.Any() &&
                    corpContacts.Distinct(new ContactsComparer()).Count() == 1)
                {
                    UpdateContactInAmo(_corpRepo, FieldLists.ContactCorp, _client, (int)corpContacts.First().id);
                }
                else if (corpContacts.Any() &&
                    corpContacts.Distinct(new ContactsComparer()).Count() > 1)
                {
                    _log.Add($"Check for doubles: {JsonConvert.SerializeObject(corpContacts.Distinct(new ContactsComparer()), Formatting.Indented)}");
                    CreateContactInAmo(_corpRepo, FieldLists.ContactCorp, _client);
                }
                else
                {
                    CreateContactInAmo(_corpRepo, FieldLists.ContactCorp, _client);
                }
                #endregion

                _1CRepo.UpdateClient(_client);
            }
            catch (Exception e)
            {
                _log.Add($"Error in CreateOrUpdateAmoContact: {e}");
            }
        }
    }
}