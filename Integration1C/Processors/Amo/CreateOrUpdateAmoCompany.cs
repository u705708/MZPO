using MZPO.AmoRepo;
using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class CreateOrUpdateAmoCompany
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Company1C _company1C;
        private readonly IAmoRepo<Company> _compRepo;
        private readonly int _amo_acc;
        private readonly RecentlyUpdatedEntityFilter _filter;

        public CreateOrUpdateAmoCompany(Company1C company1C, Amo amo, Log log, RecentlyUpdatedEntityFilter filter)
        {
            _amo = amo;
            _log = log;
            _company1C = company1C;
            _compRepo = _amo.GetAccountById(19453687).GetRepo<Company>();
            _amo_acc = 19453687;
            _filter = filter;
        }

        class CompaniesComparer : IEqualityComparer<Company>
        {
            public bool Equals(Company x, Company y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (x is null || y is null)
                    return false;

                return x.id == y.id;
            }

            public int GetHashCode(Company c)
            {
                if (c is null) return 0;

                int hashProductCode = c.id.GetHashCode();

                return hashProductCode;
            }
        }

        private static void AddUIDToEntity(Company1C company1C, int amo_acc, Company company)
        {
            company.custom_fields_values.Add(new Custom_fields_value()
            {
                field_id = FieldLists.Companies[amo_acc]["company_id_1C"],
                values = new Custom_fields_value.Values[] { new Custom_fields_value.Values() { value = company1C.company_id_1C.Value.ToString("D") } }
            });
        }

        private static void PopulateCFs(Company1C company1C, int amo_acc, Company company)
        {
            foreach (var p in company1C.GetType().GetProperties())
                if (FieldLists.Companies[amo_acc].ContainsKey(p.Name) &&
                    p.GetValue(company1C) is not null)
                {
                    try { if ((string)p.GetValue(company1C) == "") continue; }
                    catch { }

                    company.custom_fields_values.Add(new Custom_fields_value()
                    {
                        field_id = FieldLists.Companies[amo_acc][p.Name],
                        values = new Custom_fields_value.Values[] { new Custom_fields_value.Values() { value = p.GetValue(company1C) } }
                    });
                }
        }

        private static void UpdateCompanyInAmo(Company1C company1C, IAmoRepo<Company> compRepo, int company_id, int amo_acc, RecentlyUpdatedEntityFilter filter)
        {
            Company company = new()
            {
                id = company_id,
                //name = company1C.name,
                custom_fields_values = new()
            };

            AddUIDToEntity(company1C, amo_acc, company);

            PopulateCFs(company1C, amo_acc, company);

            try
            {
                filter.AddEntity(company_id);
                compRepo.Save(company);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update company {company_id} in amo: {e.Message}");
            }
        }

        private static int CreateCompanyInAmo(Company1C company1C, IAmoRepo<Company> compRepo, int amo_acc, RecentlyUpdatedEntityFilter filter)
        {
            Company company = new()
            {
                name = company1C.name,
                custom_fields_values = new()
            };

            AddUIDToEntity(company1C, amo_acc, company);

            PopulateCFs(company1C, amo_acc, company);

            try
            {
                var result = compRepo.AddNew(company).ToList();
                result.ForEach(x => filter.AddEntity(x.id));
                if (result.Any())
                    return result.First().id;
                else throw new Exception("Amo returned no company Ids.");
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to create company in amo: {e.Message}");
            }
        }

        public List<Amo_id> Run()
        {
            if (_company1C.amo_ids is null) _company1C.amo_ids = new();

            try
            {
                #region Checking if company already linked to entity and updating if possible
                if (_company1C.amo_ids.Any(x => x.account_id == _amo_acc))
                {
                    foreach (var c in _company1C.amo_ids.Where(x => x.account_id == _amo_acc))
                        try 
                        { 
                            UpdateCompanyInAmo(_company1C, _compRepo, c.entity_id, _amo_acc, _filter);

                            _log.Add($"Updated company {c.entity_id} in amo.");

                            return _company1C.amo_ids;
                        }
                        catch (Exception e) 
                        { 
                            _log.Add($"Unable to update company {c.entity_id} in amo {e}"); 
                        }
                } 
                #endregion

                #region Checking company
                List<Company> similarCompanies = new();
                if (_company1C.INN is not null &&
                    _company1C.INN != "")
                    similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_company1C.INN.Trim().Replace(" ", "")}"));

                if (_company1C.phone is not null &&
                    _company1C.phone != "")
                    similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_company1C.phone.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "")}"));

                if (_company1C.email is not null &&
                    _company1C.email != "")
                    similarCompanies.AddRange(_compRepo.GetByCriteria($"query={_company1C.email.Trim().Replace(" ", "")}"));
                #endregion

                #region Updating found company
                if (similarCompanies.Any())
                {
                    UpdateCompanyInAmo(_company1C, _compRepo, similarCompanies.First().id, _amo_acc, _filter);
                    _company1C.amo_ids.Add(new()
                    {
                        account_id = _amo_acc,
                        entity_id = similarCompanies.First().id
                    });

                    _log.Add($"Found and updated company {similarCompanies.First().id} in amo");

                    return _company1C.amo_ids;
                }

                if (similarCompanies.Distinct(new CompaniesComparer()).Count() > 1)
                    _log.Add($"Check for doubles: {JsonConvert.SerializeObject(similarCompanies.Distinct(new CompaniesComparer()).Select(x => new { x.id, x.account_id }), Formatting.Indented)}");
                #endregion

                #region Creating new company
                var compId = CreateCompanyInAmo(_company1C, _compRepo, _amo_acc, _filter);

                if (_company1C.amo_ids is null) _company1C.amo_ids = new();
                _company1C.amo_ids.Add(new()
                {
                    account_id = 19453687,
                    entity_id = compId
                });
                #endregion

                _log.Add($"Created company {compId} in amo.");
            }
            catch (Exception e)
            {
                _log.Add($"Unable to update company in amo from 1C: {e.Message}");
            }

            return _company1C.amo_ids;
        }
    }
}