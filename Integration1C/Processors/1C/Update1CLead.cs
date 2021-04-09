using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;

namespace Integration1C
{
    public class Update1CLead
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly int _leadId;
        private readonly int _amo_acc;
        private readonly Cred1C _cred1C;

        public Update1CLead(Amo amo, Log log, int leadId, int amo_acc, Cred1C cred1C)
        {
            _amo = amo;
            _log = log;
            _leadId = leadId;
            _amo_acc = amo_acc;
            _cred1C = cred1C;
        }

        private static void PopulateCFs(Lead lead, int amo_acc, Lead1C lead1C)
        {
            if (lead.custom_fields_values is not null)
                foreach (var p in lead1C.GetType().GetProperties())
                    if (FieldLists.Leads[amo_acc].ContainsKey(p.Name) &&
                        lead.custom_fields_values.Any(x => x.field_id == FieldLists.Leads[amo_acc][p.Name]))
                    {
                        var value = lead.custom_fields_values.First(x => x.field_id == FieldLists.Leads[amo_acc][p.Name]).values[0].value;
                        if ((p.PropertyType == typeof(Guid?) ||
                            p.PropertyType == typeof(Guid)) &&
                            Guid.TryParse((string)value, out Guid guidValue))
                        {
                            p.SetValue(lead1C, guidValue);
                            continue;
                        }

                        p.SetValue(lead1C, value);
                    }
        }

        private static void UpdateLeadIn1C(Amo amo, Log log, Lead lead, Guid lead_id_1C, int amo_acc, Cred1C cred1C)
        {
            var repo1C = new LeadRepository(cred1C);

            Lead1C lead1C = repo1C.GetLead(lead_id_1C);
            
            if (lead1C == default) throw new Exception($"Unable to update lead in 1C. 1C returned no lead {lead_id_1C}.");

            PopulateCFs(lead, amo_acc, lead1C);

            lead1C.responsible_user = UserList.Get1CUser(lead.responsible_user_id);

            if (string.IsNullOrEmpty(lead1C.lead_status))
                lead1C.lead_status = "ВРаботе";

            if (amo_acc == 19453687)
                lead1C.is_corporate = true;

            repo1C.UpdateLead(lead1C);
        }

        public Guid Run()
        {
            try
            {
                var leadRepo = _amo.GetAccountById(_amo_acc).GetRepo<Lead>();

                var lead = leadRepo.GetById(_leadId);

                if (lead is null ||
                    lead == default)
                    throw new Exception("No lead returned from amo.");

                Guid lead_id_1C = default;

                if (lead.custom_fields_values is not null &&
                    lead.custom_fields_values.Any(x => x.field_id == FieldLists.Leads[_amo_acc]["lead_id_1C"]) &&
                    Guid.TryParse((string)lead.custom_fields_values.First(x => x.field_id == FieldLists.Leads[_amo_acc]["lead_id_1C"]).values[0].value, out lead_id_1C))
                {
                    UpdateLeadIn1C(_amo, _log, lead, lead_id_1C, _amo_acc, _cred1C);
                    _log.Add($"Updated lead in 1C {lead_id_1C}.");
                }

                return lead_id_1C;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update lead {_leadId} in 1C: {e}");
                return default;
            }
        }
    }
}