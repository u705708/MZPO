using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class CreateOrUpdate1CClientFromLead
    {
        private readonly AmoAccount _acc;
        private readonly int _lead_id;
        private readonly ClientRepository _clientRepo1C;
        private readonly Log _log;
        
        public CreateOrUpdate1CClientFromLead(int lead_id, AmoAccount acc, Log log)
        {
            _acc = acc;
            _lead_id = lead_id;
            _clientRepo1C = new();
            _log = log;
        }

        public void Run()
        {
            var leadRepo = _acc.GetRepo<MZPO.AmoRepo.Lead>();
            var contRepo = _acc.GetRepo<MZPO.AmoRepo.Contact>();

            Dictionary<string, int> fieldIds;
            if (_acc.id == 19453687) fieldIds = FieldLists.ContactCorp;
            else fieldIds = FieldLists.ContactRet;

            var lead = leadRepo.GetById(_lead_id);

            if (lead is null ||
                lead._embedded is null ||
                lead._embedded.contacts is null) return;
            foreach (var c in lead._embedded.contacts)
            {
                var contact = contRepo.GetById(c.id);
                if (contact is null) continue;
                if (contact.custom_fields_values is not null &&
                    contact.custom_fields_values.Any(x => x.field_id == fieldIds["client_id_1C"]))
                    try { _clientRepo1C.UpdateClient(Get1C.ClientFromContact(c, fieldIds)); }
                    catch (Exception e){ _log.Add($"Unable to update client in 1C: {e}"); }
                else
                {
                    int client_id = 0;

                    try { client_id = _clientRepo1C.AddClient(Get1C.ClientFromContact(c, fieldIds)).client_id_1C; }
                    catch (Exception e) { _log.Add($"Unable to update client in 1C: {e}"); }

                    if (client_id == 0) return;

                    if (contact.custom_fields_values is null) contact.custom_fields_values = new();
                    contact.custom_fields_values.Add(new()
                    {
                        field_id = fieldIds["client_id_1C"],
                        values = new MZPO.AmoRepo.Contact.Custom_fields_value.Values[] {
                            new MZPO.AmoRepo.Contact.Custom_fields_value.Values() { value = $"{client_id}" }
                        }
                    });
                    try { contRepo.Save(contact); }
                    catch (Exception e) { _log.Add($"Unable to update contact {contact.id} with 1C id {client_id}: {e}"); }
                }
            }
        }
    }
}