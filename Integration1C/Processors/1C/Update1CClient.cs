using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Update1CClient
    {
        private readonly AmoAccount _acc;
        private readonly int _contact_id;
        private readonly ClientRepository _clientRepo1C;
        private readonly Log _log;

        public Update1CClient(int contact_id, AmoAccount acc, Log log)
        {
            _acc = acc;
            _contact_id = contact_id;
            _clientRepo1C = new();
            _log = log;
        }

        public void Run()
        {
            var contRepo = _acc.GetRepo<MZPO.AmoRepo.Contact>();

            Dictionary<string, int> fieldIds;
            if (_acc.id == 19453687) fieldIds = FieldLists.ContactCorp;
            else fieldIds = FieldLists.ContactRet;

            var contact = contRepo.GetById(_contact_id);
            if (contact is not null &&
                contact.custom_fields_values is not null &&
                contact.custom_fields_values.Any(x => x.field_id == fieldIds["client_id_1C"]))
                try { _clientRepo1C.UpdateClient(Get1C.ClientFromContact(contact, fieldIds)); }
                catch (Exception e) { _log.Add($"Unable to update client in 1C: {e}"); }
        }
    }
}