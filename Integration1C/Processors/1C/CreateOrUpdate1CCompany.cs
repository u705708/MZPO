using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;

namespace Integration1C
{
    public class CreateOrUpdate1CCompany
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly int _leadId;
        private readonly int _amo_acc;
        private readonly CompanyRepository _repo1C;

        public CreateOrUpdate1CCompany(Amo amo, Log log, int leadId, Cred1C cred1C)
        {
            _amo = amo;
            _log = log;
            _leadId = leadId;
            _amo_acc = 19453687;
            _repo1C = new(cred1C);
        }

        private static void PopulateCFs(Company company, int amo_acc, Company1C company1C)
        {
            if (company.custom_fields_values is not null)
                foreach (var p in company1C.GetType().GetProperties())
                    if (FieldLists.Companies[amo_acc].ContainsKey(p.Name) &&
                        company.custom_fields_values.Any(x => x.field_id == FieldLists.Companies[amo_acc][p.Name]))
                    {
                        var value = company.custom_fields_values.First(x => x.field_id == FieldLists.Companies[amo_acc][p.Name]).values[0].value;
                        if (p.PropertyType == typeof(Guid?) &&
                            Guid.TryParse((string)value, out Guid guidValue))
                        {
                            p.SetValue(company1C, guidValue);
                            continue;
                        }

                        p.SetValue(company1C, value);
                    }
        }

        private static void UpdateCompanyIn1C(Company company, Guid company_id_1C, int amo_acc, CompanyRepository repo1C)
        {
            Company1C company1C = repo1C.GetCompany(company_id_1C);

            if (company1C == default) throw new Exception($"Unable to update company in 1C. 1C returned no company {company_id_1C}.");

            PopulateCFs(company, amo_acc, company1C);

            repo1C.UpdateCompany(company1C);
        }

        private static Guid CreateCompanyIn1C(Company company, int amo_acc, CompanyRepository repo1C)
        {
            Company1C company1C = new() {
                name = company.name,
                amo_ids = new() { new() {
                        account_id = amo_acc,
                        entity_id = company.id
            } } };

            PopulateCFs(company, amo_acc, company1C);

            var result = repo1C.AddCompany(company1C);
            if (result == default)
                throw new Exception($"Unable to create company in 1C: returned empty guid.");
            return result;
        }

        public Guid Run()
        {
            try
            {
                var compRepo = _amo.GetAccountById(_amo_acc).GetRepo<Company>();

                #region Retrieving company
                Lead lead = _amo.GetAccountById(_amo_acc).GetRepo<Lead>().GetById(_leadId);

                if (lead._embedded is null ||
                    lead._embedded.companies is null ||
                    !lead._embedded.companies.Any())
                {
                    throw new Exception($"Unable to update company in 1C, no company linked to lead {_leadId}.");
                }

                Company company = compRepo.GetById(lead._embedded.companies.First().id);
                #endregion

                #region Checking if company exists in 1C and updating if possible
                if (company.custom_fields_values is not null &&
                    company.custom_fields_values.Any(x => x.field_id == FieldLists.Companies[_amo_acc]["company_id_1C"]) &&
                    Guid.TryParse((string)company.custom_fields_values.First(x => x.field_id == FieldLists.Companies[_amo_acc]["company_id_1C"]).values[0].value, out Guid company_id_1C))
                {
                    UpdateCompanyIn1C(company, company_id_1C, _amo_acc, _repo1C);

                    _log.Add($"Updated company in 1C {company_id_1C}.");
                    
                    return company_id_1C;
                }
                #endregion

                #region Creating company in 1C
                var result = CreateCompanyIn1C(company, _amo_acc, _repo1C);

                Company updatedCompany = new()
                {
                    id = company.id,
                    custom_fields_values = new()
                    {
                        new()
                        {
                            field_id = FieldLists.Companies[_amo_acc]["company_id_1C"],
                            values = new Company.Custom_fields_value.Values[] { new Company.Custom_fields_value.Values() { value = result.ToString("D") } }
                        }
                    }
                };

                _log.Add($"Created company in 1C {result}.");
                #endregion

                compRepo.Save(updatedCompany);

                return result;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to create or update company from lead {_leadId} in 1C: {e}");
                return default;
            }
        }
    }
}