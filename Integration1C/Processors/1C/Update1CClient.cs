using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;

namespace Integration1C
{
    public class Update1CClient
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly int _contactId;
        private readonly int _amo_acc;
        private readonly ClientRepository _repo1C;

        public Update1CClient(Amo amo, Log log, int contactId, int amo_acc, Cred1C cred1C)
        {
            _amo = amo;
            _log = log;
            _contactId = contactId;
            _amo_acc = amo_acc;
            _repo1C = new(cred1C);
        }

        private static void PopulateClientCFs(Contact contact, int amo_acc, Client1C client1C)
        {
            if (contact.custom_fields_values is not null)
                foreach (var p in client1C.GetType().GetProperties())
                    if (FieldLists.Contacts[amo_acc].ContainsKey(p.Name) &&
                        contact.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[amo_acc][p.Name]))
                        p.SetValue(client1C, contact.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[amo_acc][p.Name]).values[0].value);
        }

        private static void UpdateClientIn1C(Guid client_id_1C, Contact contact, int amo_acc, Amo amo, Log log, ClientRepository repo1C)
        {
            var client1C = repo1C.GetClient(client_id_1C);
            if (client1C == default) throw new Exception($"Unable to add client to 1C. 1C returned no client {client_id_1C}.");

            PopulateClientCFs(contact, amo_acc, client1C);

            repo1C.UpdateClient(client1C);

            new UpdateAmoContact(client1C, amo, log).Run();
        }

        public void Run()
        {
            try
            {
                #region Getting contact
                var contRepo = _amo.GetAccountById(_amo_acc).GetRepo<Contact>();

                var contact = contRepo.GetById(_contactId);

                if (contact.custom_fields_values is null)
                    throw new Exception($"No suitable contacts to add to 1C at contact {_contactId}");
                #endregion

                #region Check for UIDs
                if (contact.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]) &&
                    Guid.TryParse((string)contact.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]).values[0].value, out Guid client_id_1C))
                {
                    UpdateClientIn1C(client_id_1C, contact, _amo_acc, _amo, _log, _repo1C);
                }
                #endregion
            }
            catch (Exception e)
            {
                _log.Add($"Unable to create or updae contact in 1C from lead {_contactId}: {e}");
            }
        }
    }
}