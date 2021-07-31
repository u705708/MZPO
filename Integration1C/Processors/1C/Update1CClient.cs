using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
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
        private readonly RecentlyUpdatedEntityFilter _filter;

        public Update1CClient(Amo amo, Log log, int contactId, int amo_acc, Cred1C cred1C, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _contactId = contactId;
            _amo_acc = amo_acc;
            _repo1C = new(cred1C);
            _filter = filter;
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
                            p.SetValue(client1C, new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0));
                            continue;
                        }

                        p.SetValue(client1C, value);
                    }
        }

        private static Client1C UpdateClientIn1C(Guid client_id_1C, Contact contact, int amo_acc, ClientRepository repo1C)
        {
            var client1C = repo1C.GetClient(client_id_1C);
            if (client1C == default) throw new Exception($"1C ERROR: Unable to update client to 1C. 1C returned default client_id.");

            PopulateClientCFs(contact, amo_acc, client1C);

            client1C.name = contact.name;

            repo1C.UpdateClient(client1C);

            return client1C;
        }

        public Guid Run()
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
                Guid client_id_1C = default;

                if (contact.custom_fields_values.Any(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]) &&
                    Guid.TryParse((string)contact.custom_fields_values.First(x => x.field_id == FieldLists.Contacts[_amo_acc]["client_id_1C"]).values[0].value, out client_id_1C))
                {
                    var client1C = UpdateClientIn1C(client_id_1C, contact, _amo_acc, _repo1C);
                    _log.Add($"Updated client in 1C {client_id_1C}.");

                    _filter.AddEntity(_contactId);

                    new UpdateAmoContact(client1C, _amo, _log, _filter).Run();
                }
                #endregion

                return client_id_1C;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update client in 1C from contact {_contactId}: {e.Message}");
                return default;
            }
        }
    }
}