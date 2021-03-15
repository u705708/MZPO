using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class CreateOrUpdate1CLeadWithContacts
    {
        private readonly AmoAccount _acc;
        private readonly Amo _amo;
        private readonly int _lead_id;
        private readonly LeadRepository _leadRepo1C;
        private readonly Log _log;

        public CreateOrUpdate1CLeadWithContacts(int lead_id, Amo amo, AmoAccount acc, Log log)
        {
            _amo = amo;
            _acc = acc;
            _lead_id = lead_id;
            _leadRepo1C = new();
            _log = log;
        }

        private static Lead1C Get1CLead(Lead lead, AmoAccount acc, Amo amo, Log log)
        {
            Lead1C result = new() { lead_id_1C = default, client_id_1C = default, is_corporate = false };

            #region Prepare field dictionaries
            Dictionary<string, int> leadFieldIds = FieldLists.LeadRet;
            Dictionary<string, int> contactFieldIds = FieldLists.ContactRet;
            Dictionary<string, int> companyFieldIds = FieldLists.CompanyRet;
            if (acc.id == 19453687)
            {
                leadFieldIds = FieldLists.LeadCorp;
                contactFieldIds = FieldLists.ContactCorp;
                companyFieldIds = FieldLists.CompanyCorp;

                result.is_corporate = true;
            }
            #endregion

            #region GetClientId
            if (lead._embedded is not null &&
                lead._embedded.contacts is not null &&
                lead._embedded.contacts.Any())
            {
                var contRepo = acc.GetRepo<Contact>();
                var contact = contRepo.GetById((int)lead._embedded.contacts.First().id);

                if (contact.custom_fields_values is not null &&
                    !contact.custom_fields_values.Any(x => x.field_id == contactFieldIds["client_id_1C"]))
                {
                    new CreateOrUpdate1CClientFromLead(lead.id, acc.id, amo, log).Run();
                    contact = contRepo.GetById((int)lead._embedded.contacts.First().id);
                }

                if (contact.custom_fields_values is not null &&
                    contact.custom_fields_values.Any(x => x.field_id == contactFieldIds["client_id_1C"]))
                {
                    Guid.TryParse((string)contact.custom_fields_values.First(x => x.field_id == contactFieldIds["client_id_1C"]).values[0].value, out Guid value);
                    result.client_id_1C = value;
                }
            }
            #endregion

            #region GetCompanyId
            if (lead._embedded is not null &&
                lead._embedded.companies is not null &&
                lead._embedded.companies.Any())
            {
                var compRepo = acc.GetRepo<Company>();
                var company = compRepo.GetById(lead._embedded.companies.First().id);

                if (company.custom_fields_values is not null &&
                    !company.custom_fields_values.Any(x => x.field_id == companyFieldIds["company_id_1C"]))
                {
                    new CreateOrUpdate1CCompanyFromLead(lead.id, amo, log).Run();
                    company = compRepo.GetById(lead._embedded.companies.First().id);
                }

                if (company.custom_fields_values is not null &&
                    company.custom_fields_values.Any(x => x.field_id == companyFieldIds["company_id_1C"]))
                {
                    Guid.TryParse((string)company.custom_fields_values.First(x => x.field_id == companyFieldIds["company_id_1C"]).values[0].value, out Guid value);
                    result.company_id_1C = value;
                }
            }
            #endregion

            result.price = (int)lead.price;
            result.author = lead.created_by.ToString();
            result.responsible_user = lead.responsible_user_id.ToString();
            result.lead_status = lead.status_id.ToString();

            if (lead.custom_fields_values is not null)
            {
                if (lead.custom_fields_values.Any(x => x.field_id == leadFieldIds["organization"]))
                    result.organization = (string)lead.custom_fields_values.First(x => x.field_id == leadFieldIds["organization"]).values[0].value;

                if (lead.custom_fields_values.Any(x => x.field_id == leadFieldIds["lead_id_1C"]))
                {
                    Guid.TryParse((string)lead.custom_fields_values.First(x => x.field_id == leadFieldIds["lead_id_1C"]).values[0].value, out Guid value);
                    result.lead_id_1C = value;
                }

                if (lead.custom_fields_values.Any(x => x.field_id == leadFieldIds["marketing_channel"]))
                    result.marketing_channel = (string)lead.custom_fields_values.First(x => x.field_id == leadFieldIds["marketing_channel"]).values[0].value;

                if (lead.custom_fields_values.Any(x => x.field_id == leadFieldIds["marketing_source"]))
                    result.marketing_source = (string)lead.custom_fields_values.First(x => x.field_id == leadFieldIds["marketing_source"]).values[0].value;
            }

            return result;
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
                try { _leadRepo1C.UpdateLead(Get1CLead(lead, _acc, _amo, _log)); }
                catch (Exception e) { _log.Add($"Unable to update lead in 1C: {e}"); }
            else
            {
                Guid lead_id_1C = default;

                try { lead_id_1C = _leadRepo1C.AddLead(Get1CLead(lead, _acc, _amo, _log)).lead_id_1C; }
                catch (Exception e) { _log.Add($"Unable to update lead in 1C: {e}"); }

                if (lead_id_1C == default) return;

                if (lead.custom_fields_values is null) lead.custom_fields_values = new();
                lead.custom_fields_values.Add(new()
                {
                    field_id = fieldIds["lead_id_1C"],
                    values = new Lead.Custom_fields_value.Values[] {
                        new Lead.Custom_fields_value.Values() { value = $"{lead_id_1C}" }
                    }
                });
                try { leadRepo.Save(lead); }
                catch (Exception e) { _log.Add($"Unable to update lead {lead.id} with 1C id {lead_id_1C}: {e}"); }
            }
        }
    }
}