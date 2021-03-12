using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class CreateOrUpdate1CCompanyFromLead
    {
        private readonly AmoAccount _acc;
        private readonly int _lead_id;
        private readonly CompanyRepository _companyRepo1C;
        private readonly Log _log;

        public CreateOrUpdate1CCompanyFromLead(int lead_id, AmoAccount acc, Log log)
        {
            _acc = acc;
            _lead_id = lead_id;
            _companyRepo1C = new();
            _log = log;
        }

        public void Run()
        {
            var leadRepo = _acc.GetRepo<Lead>();
            var compRepo = _acc.GetRepo<Company>();

            var lead = leadRepo.GetById(_lead_id);

            if (lead._embedded is null ||
                lead._embedded.companies is null)
                return;

            Dictionary<string, int> fieldIds;
            if (_acc.id == 19453687) fieldIds = FieldLists.CompanyCorp;
            else fieldIds = FieldLists.CompanyRet;

            var company = compRepo.GetById(lead._embedded.companies.First().id);
            if (company is null) return;
            if (company.custom_fields_values is not null &&
                company.custom_fields_values.Any(x => x.field_id == fieldIds["company_id_1C"]))
                try { _companyRepo1C.UpdateCompany(Get1C.CompanyFromCompany(company, fieldIds)); }
                catch (Exception e) { _log.Add($"Unable to update company in 1C: {e}"); }
            else
            {
                Guid company_id = default;

                try { company_id = _companyRepo1C.AddCompany(Get1C.CompanyFromCompany(company, fieldIds)).Company_id_1C; }
                catch (Exception e) { _log.Add($"Unable to update company in 1C: {e}"); }

                if (company_id == default) return;

                if (company.custom_fields_values is null) company.custom_fields_values = new();
                company.custom_fields_values.Add(new()
                {
                    field_id = fieldIds["company_id_1C"],
                    values = new Company.Custom_fields_value.Values[] {
                        new Company.Custom_fields_value.Values() { value = $"{company_id}" }
                    }
                });
                try { compRepo.Save(company); }
                catch (Exception e) { _log.Add($"Unable to update company {company.id} with 1C id {company_id}: {e}"); }
            }
        }
    }
}