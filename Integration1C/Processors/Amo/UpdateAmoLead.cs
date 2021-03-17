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

        public UpdateAmoLead(Lead1C lead1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _lead1C = lead1C;

        }

        private static void AddUIDToEntity(Lead1C lead1C, int acc_id, Lead lead)
        {
            lead.custom_fields_values.Add(new Lead.Custom_fields_value()
            {
                field_id = FieldLists.Leads[acc_id]["lead_id_1C"],
                values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = lead1C.lead_id_1C.Value.ToString("D") } }
            });
        }

        private static void PopulateCFs(Lead1C lead1C, int acc_id, Lead lead)
        {
            foreach (var p in lead1C.GetType().GetProperties())
                if (FieldLists.Leads[acc_id].ContainsKey(p.Name) &&
                    p.GetValue(lead1C) is not null &&
                    (string)p.GetValue(lead1C) != "") //В зависимости от политики передачи пустых полей
                {
                    lead.custom_fields_values.Add(new Lead.Custom_fields_value()
                    {
                        field_id = FieldLists.Leads[acc_id][p.Name],
                        values = new Lead.Custom_fields_value.Values[] { new Lead.Custom_fields_value.Values() { value = (string)p.GetValue(lead1C) } }
                    });
                }
        }

        private static void UpdateLeadInAmo(Lead1C lead1C, IAmoRepo<Lead> leadRepo, int lead_id, int acc_id)
        {
            Lead lead = new()
            {
                id = lead_id,
                price = lead1C.price,
                custom_fields_values = new(),
            };

            AddUIDToEntity(lead1C, acc_id, lead);

            PopulateCFs(lead1C, acc_id, lead);

            try
            {
                leadRepo.Save(lead);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update lead {lead_id} in amo: {e}");
            }
        }

        public List<Amo_id> Run()
        {
            try
            {
                int amo_acc = 28395871;
                if (_lead1C.is_corporate) amo_acc = 19453687;

                var leadRepo = _amo.GetAccountById(amo_acc).GetRepo<Lead>();

                if (_lead1C.amo_ids.Any(x => x.account_id == amo_acc))
                    UpdateLeadInAmo(_lead1C, leadRepo, _lead1C.amo_ids.First().entity_id, amo_acc);
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update lead {_lead1C.lead_id_1C} in amo: {e}");
            }

            return _lead1C.amo_ids;
        }
    }
}