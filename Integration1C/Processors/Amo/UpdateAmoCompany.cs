using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class UpdateAmoCompany
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Company1C _company1C;
        private readonly IAmoRepo<Company> _compRepo;
        private readonly int _amo_acc;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public UpdateAmoCompany(Company1C company1C, Amo amo, Log log, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _company1C = company1C;
            _amo_acc = 19453687;
            _compRepo = _amo.GetAccountById(_amo_acc).GetRepo<Company>();
            _filter = filter;
        }

        private static void AddUIDToEntity(Company1C company1C, Company company)
        {
            company.custom_fields_values.Add(new Custom_fields_value()
            {
                field_id = FieldLists.Companies[19453687]["company_id_1C"],
                values = new Custom_fields_value.Value[] { new Custom_fields_value.Value() { value = company1C.company_id_1C.Value.ToString("D") } }
            });
        }

        private static void PopulateCFs(Company1C company1C, Company company)
        {
            foreach (var p in company1C.GetType().GetProperties())
                if (FieldLists.Companies[19453687].ContainsKey(p.Name) &&
                    p.GetValue(company1C) is not null)
                {
                    try { if ((string)p.GetValue(company1C) == "") continue; }
                    catch { }

                    if (company.custom_fields_values is null) company.custom_fields_values = new();
                    company.custom_fields_values.Add(new Custom_fields_value()
                    {
                        field_id = FieldLists.Companies[19453687][p.Name],
                        values = new Custom_fields_value.Value[] { new Custom_fields_value.Value() { value = p.GetValue(company1C) } }
                    });
                }
        }

        private static void UpdateCompanyInAmo(Company1C company1C, IAmoRepo<Company> compRepo, int company_id, RecentlyUpdatedEntityFilter filter)
        {
            Company company = new()
            {
                id = company_id,
                //name = company1C.name,
                custom_fields_values = new()
            };

            AddUIDToEntity(company1C, company);

            PopulateCFs(company1C, company);

            try
            {
                filter.AddEntity(company_id);
                compRepo.Save(company);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update company {company_id} in amo: {e}");
            }
        }

        public List<Amo_id> Run()
        {
            try
            {
                if (_company1C.amo_ids is not null &&
                    _company1C.amo_ids.Any(x => x.account_id == _amo_acc))
                    foreach (var c in _company1C.amo_ids.Where(x => x.account_id == _amo_acc))
                    {
                        UpdateCompanyInAmo(_company1C, _compRepo, c.entity_id, _filter);
                        _log.Add($"Company {c.entity_id} updated in amo.");
                    }
                
                return _company1C.amo_ids;
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update company in amo from 1C: {e.Message}");
                return new();
            }
        }
    }
}