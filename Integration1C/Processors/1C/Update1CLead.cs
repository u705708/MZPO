using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Update1CLead
    {
        private readonly AmoAccount _acc;
        private readonly int _lead_id;
        private readonly LeadRepository _leadRepo1C;
        private readonly Log _log;

        public Update1CLead(int lead_id, AmoAccount acc, Log log)
        {
            _acc = acc;
            _lead_id = lead_id;
            _leadRepo1C = new();
            _log = log;
        }

        private static Lead1C Get1CLead(Lead lead, Lead1C lead1C, AmoAccount acc)
        {
            #region Prepare field dictionaries
            Dictionary<string, int> leadFieldIds = FieldLists.LeadRet;
            if (acc.id == 19453687)
            {
                leadFieldIds = FieldLists.LeadCorp;
            }
            #endregion

            lead1C.price = (int)lead.price;
            lead1C.author = lead.created_by.ToString();
            lead1C.responsible_user = lead.responsible_user_id.ToString();
            lead1C.lead_status = lead.status_id.ToString();

            if (lead.custom_fields_values is not null)
            {
                if (lead.custom_fields_values.Any(x => x.field_id == leadFieldIds["organization"]))
                    lead1C.organization = (string)lead.custom_fields_values.First(x => x.field_id == leadFieldIds["organization"]).values[0].value;

                if (lead.custom_fields_values.Any(x => x.field_id == leadFieldIds["marketing_channel"]))
                    lead1C.marketing_channel = (string)lead.custom_fields_values.First(x => x.field_id == leadFieldIds["marketing_channel"]).values[0].value;

                if (lead.custom_fields_values.Any(x => x.field_id == leadFieldIds["marketing_source"]))
                    lead1C.marketing_source = (string)lead.custom_fields_values.First(x => x.field_id == leadFieldIds["marketing_source"]).values[0].value;
            }

            return lead1C;
        }

        public void Run()
        {
            var leadRepo = _acc.GetRepo<Lead>();

            var lead = leadRepo.GetById(_lead_id);

            if (lead is null) return;

            Dictionary<string, int> fieldIds = FieldLists.LeadRet;
            if (_acc.id == 19453687)
                fieldIds = FieldLists.LeadCorp;

            if (lead.custom_fields_values is not null &&
                lead.custom_fields_values.Any(x => x.field_id == fieldIds["lead_id_1C"]))
                try { _leadRepo1C.UpdateLead(Get1CLead(
                        lead, 
                        _leadRepo1C.GetLead((int)lead.custom_fields_values.First(x => x.field_id == fieldIds["lead_id_1C"]).values[0].value), 
                        _acc
                    )); }
                catch (Exception e) { _log.Add($"Unable to update lead in 1C: {e}"); }
        }
    }
}