using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class UpdateAmoLead
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Lead1C _lead1C;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public UpdateAmoLead(Lead1C lead1C, Amo amo, Log log, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _lead1C = lead1C;
            _filter = filter;
        }

        private static void AddUIDToEntity(Lead1C lead1C, int acc_id, Lead lead)
        {
            lead.custom_fields_values.Add(new Custom_fields_value()
            {
                field_id = FieldLists.Leads[acc_id]["lead_id_1C"],
                values = new Custom_fields_value.Value[] { new Custom_fields_value.Value() { value = lead1C.lead_id_1C.Value.ToString("D") } }
            });
        }

        private static void PopulateCFs(Lead1C lead1C, int acc_id, Lead lead)
        {
            foreach (var p in lead1C.GetType().GetProperties())
                if (FieldLists.Leads[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(lead1C) is not null)
                {
                    try { if ((string)p.GetValue(lead1C) == "") continue; }
                    catch { }

                    lead.custom_fields_values.Add(new Custom_fields_value()
                    {
                        field_id = FieldLists.Leads[acc_id][p.Name],
                        values = new Custom_fields_value.Value[] { new Custom_fields_value.Value() { value = p.GetValue(lead1C) } }
                    });
                }
        }

        private static void UpdateLeadInAmo(Lead1C lead1C, IAmoRepo<Lead> leadRepo, int lead_id, int acc_id, RecentlyUpdatedEntityFilter filter)
        {
            Lead lead = new()
            {
                id = lead_id,
                price = lead1C.price,
                responsible_user_id = UserList.GetAmoUser(lead1C.responsible_user),
                custom_fields_values = new(),
            };

            if (lead.responsible_user_id is null) 
                lead.responsible_user_id = UserList.GetAmoUser(lead1C.author);

            AddUIDToEntity(lead1C, acc_id, lead);

            PopulateCFs(lead1C, acc_id, lead);

            try
            {
                filter.AddEntity(lead_id);
                leadRepo.Save(lead);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update lead {lead_id} in amo: {e.Message}");
            }
        }

        public List<Amo_id> Run()
        {
            try
            {
                int amo_acc = 28395871;
                if (_lead1C.is_corporate) amo_acc = 19453687;
                if (_lead1C.organization == "ООО «Первый Профессиональный Институт Эстетики»") amo_acc = 29490250;

                var leadRepo = _amo.GetAccountById(amo_acc).GetRepo<Lead>();

                if (_lead1C.amo_ids.Any(x => x.account_id == amo_acc))
                    UpdateLeadInAmo(_lead1C, leadRepo, _lead1C.amo_ids.First().entity_id, amo_acc, _filter);

                _log.Add($"Updated lead {_lead1C.amo_ids.First().entity_id} in amo {amo_acc}.");
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update lead {_lead1C.lead_id_1C} in amo: {e.Message}");
            }

            return _lead1C.amo_ids;
        }
    }
}