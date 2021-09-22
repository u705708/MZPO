using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class UpdateAmoIds
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly ClientRepository _repo1C;

        public UpdateAmoIds(Amo amo, Log log, Cred1C cred1C)
        {
            _amo = amo;
            _log = log;
            _repo1C = new(cred1C);
        }

        public void Run(Contact contact)
        {
            var client_id_1C = contact.GetCFStringValue(710429);

            try
            {
                if (string.IsNullOrEmpty(client_id_1C) ||
                    !Guid.TryParse(client_id_1C, out Guid clientId))
                    return;

                var client1C = _repo1C.GetClient(clientId);

                if (client1C.amo_ids is null)
                    client1C.amo_ids = new();

                if (!client1C.amo_ids.Any(x => x.account_id == contact.account_id))
                {
                    client1C.amo_ids.Add(new() { account_id = (int)contact.account_id, entity_id = (int)contact.id });
                    _repo1C.UpdateClient(client1C);
                    return;
                }

                if (!client1C.amo_ids.Any(x => x.entity_id == contact.id))
                {
                    client1C.amo_ids.First(x => x.account_id == contact.account_id).entity_id = (int)contact.id;
                    _repo1C.UpdateClient(client1C);
                    return;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to update amoIds fo client {client_id_1C}: {e.Message}");
            }
        }
    }
}