using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;

namespace Integration1C
{
    public class Update1CCompany
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly int _companyId;
        private readonly int _amo_acc;
        private readonly CompanyRepository _repo1C;

        public Update1CCompany(Amo amo, Log log, int companyId, Cred1C cred1C)
        {
            _amo = amo;
            _log = log;
            _companyId = companyId;
            _amo_acc = 19453687;
            _repo1C = new(cred1C);
        }

        private static void PopulateCFs(Company company, int amo_acc, Company1C company1C)
        {
            if (company.custom_fields_values is not null)
                foreach (var p in company1C.GetType().GetProperties())
                    if (FieldLists.Companies[amo_acc].ContainsKey(p.Name) &&
                        company.custom_fields_values.Any(x => x.field_id == FieldLists.Companies[amo_acc][p.Name]))
                        p.SetValue(company1C, company.custom_fields_values.First(x => x.field_id == FieldLists.Companies[amo_acc][p.Name]).values[0].value);
        }

        private static void UpdateCompanyIn1C(Company company, Guid company_id_1C, int amo_acc, CompanyRepository repo1C)
        {
            Company1C company1C = new() {
                name = company.name,
                company_id_1C = company_id_1C,
                amo_ids = new() { new() {
                        account_id = amo_acc,
                        entity_id = company.id
            } } };

            PopulateCFs(company, amo_acc, company1C);

            repo1C.UpdateCompany(company1C);
        }

        public void Run()
        {
            try
            {
                var compRepo = _amo.GetAccountById(_amo_acc).GetRepo<Company>();

                #region Checking if company exists in 1C and updating if possible
                Company company = compRepo.GetById(_companyId);

                if (company.custom_fields_values is not null &&
                    company.custom_fields_values.Any(x => x.field_id == FieldLists.Companies[_amo_acc]["company_id_1C"]) &&
                    Guid.TryParse((string)company.custom_fields_values.First(x => x.field_id == FieldLists.Companies[_amo_acc]["company_id_1C"]).values[0].value, out Guid company_id_1C))
                {
                    UpdateCompanyIn1C(company, company_id_1C, _amo_acc, _repo1C);
                    _log.Add($"Updated company in 1C {company_id_1C}.");
                }
                #endregion
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update company {_companyId} in 1C: {e}");
            }
        }
    }
}