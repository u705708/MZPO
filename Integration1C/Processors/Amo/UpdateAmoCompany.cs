using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Linq;

namespace Integration1C
{
    public class UpdateAmoCompany
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Company1C _company1C;
        private readonly IAmoRepo<Company> _compRepo;

        public UpdateAmoCompany(Company1C company1C, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _company1C = company1C;
            _compRepo = _amo.GetAccountById(19453687).GetRepo<Company>();
        }

        private static void UpdateCompanyInAmo(Company1C company1C, IAmoRepo<Company> compRepo, int company_id)
        {
            Company company = new()
            {
                id = company_id,
                name = company1C.name,
                custom_fields_values = new()
            };

            company.custom_fields_values.Add(new Company.Custom_fields_value()
            {
                field_id = FieldLists.Companies[19453687]["company_id_1C"],
                values = new Company.Custom_fields_value.Values[] { new Company.Custom_fields_value.Values() { value = company1C.company_id_1C.ToString("D") } }
            });

            foreach (var p in company1C.GetType().GetProperties())
                if (FieldLists.Companies[19453687].ContainsKey(p.Name) &&
                    p.GetValue(company1C) is not null &&
                    (string)p.GetValue(company1C) != "") //В зависимости от политики передачи пустых полей
                {
                    if (company.custom_fields_values is null) company.custom_fields_values = new();
                    company.custom_fields_values.Add(new Company.Custom_fields_value()
                    {
                        field_id = FieldLists.Companies[19453687][p.Name],
                        values = new Company.Custom_fields_value.Values[] { new Company.Custom_fields_value.Values() { value = (string)p.GetValue(company1C) } }
                    });
                }
            try
            {
                compRepo.Save(company);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update company {company_id} in amo: {e}");
            }
        }

        public void Run()
        {
            try
            {
                if (_company1C.amo_ids is not null &&
                    _company1C.amo_ids.Any(x => x.account_id == 19453687))
                {
                    foreach (var c in _company1C.amo_ids.Where(x => x.account_id == 19453687))
                        UpdateCompanyInAmo(_company1C, _compRepo, c.entity_id);
                    return;
                }
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update company in amo from 1C: {e}");
            }
        }
    }
}