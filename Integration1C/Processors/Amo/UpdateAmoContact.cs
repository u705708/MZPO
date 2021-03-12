using MZPO.Services;
using MZPO.AmoRepo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Integration1C
{
    class UpdateAmoContact
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Client1C _client;
        private readonly string _request;
        private readonly IAmoRepo<Contact> _retRepo;
        private readonly IAmoRepo<Contact> _corpRepo;

        public UpdateAmoContact(string request, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _client = new();
            _request = request;
            _retRepo = _amo.GetAccountById(28395871).GetRepo<Contact>();
            _corpRepo = _amo.GetAccountById(19453687).GetRepo<Contact>();
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
            if (id == 0) return;

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

        public void Run()
        {
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(_request), _client); }
            catch (Exception e) { _log.Add($"Unable to process request: {e} --- Request: {_request}"); return; }

            #region Checking if already exist
            if (_client.Amo_ids is not null)
            {
                //UpdateContactInAmo(_retRepo, FieldLists.ContactRet, _client, _client.Amo_ids.ret_id);
                //UpdateContactInAmo(_corpRepo, FieldLists.ContactCorp, _client, _client.Amo_ids.corp_id);
                return;
            }
            #endregion
        }
    }
}