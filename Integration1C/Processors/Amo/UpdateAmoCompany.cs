using MZPO.Services;
using MZPO.AmoRepo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Integration1C
{
    class UpdateAmoCompany
    {
        private readonly Amo _amo;
        private readonly Log _log;
        private readonly Company1C _company;
        private readonly string _request;
        private readonly IAmoRepo<Company> _corpRepo;
        private readonly CompanyRepository _1CRepo;

        public UpdateAmoCompany(string request, Amo amo, Log log)
        {
            _amo = amo;
            _log = log;
            _company = new();
            _request = request;
            _corpRepo = _amo.GetAccountById(19453687).GetRepo<Company>();
            _1CRepo = new();
        }

        private static object SetFieldValue(Company company, int fieldId, object fieldValue)
        {
            if (company.custom_fields_values is null) company.custom_fields_values = new();

            if (company.custom_fields_values.Any(x => x.field_id == fieldId))
                company.custom_fields_values.First(x => x.field_id == fieldId).values[0].value = fieldValue;
            else
                company.custom_fields_values.Add(new Company.Custom_fields_value()
                {
                    field_id = fieldId,
                    values = new Company.Custom_fields_value.Values[] {
                        new Company.Custom_fields_value.Values() { value = fieldValue }
                    }
                });
            return fieldValue;
        }

        private static void UpdateCompanyInAmo(IAmoRepo<Company> repo, Dictionary<string, int> fieldList, Company1C company, int id)
        {
            if (id == 0)
            {
                return;
            }

            Company newCompany = new()
            {
                id = id,
                name = company.Name
            };

            SetFieldValue(newCompany, fieldList["phone"], company.Phone);
            SetFieldValue(newCompany, fieldList["email"], company.Email);
            SetFieldValue(newCompany, fieldList["company_id_1C"], company.Company_id_1C);
            SetFieldValue(newCompany, fieldList["LPR_name"], company.LPR_name);
            SetFieldValue(newCompany, fieldList["signee"], company.Signee);
            SetFieldValue(newCompany, fieldList["INN"], company.INN);
            SetFieldValue(newCompany, fieldList["OGRN"], company.OGRN);
            SetFieldValue(newCompany, fieldList["acc_no"], company.Acc_no);
            SetFieldValue(newCompany, fieldList["KPP"], company.KPP);
            SetFieldValue(newCompany, fieldList["BIK"], company.BIK);
            SetFieldValue(newCompany, fieldList["address"], company.Address);
            SetFieldValue(newCompany, fieldList["post_address"], company.Post_address);

            var result = repo.Save(newCompany);

            if (result.Any())
                switch (result.First().account_id)
                {
                    case 19453687:
                        if (company.Amo_ids is null) company.Amo_ids = new();
                        //company.Amo_ids.corp_id = result.First().id;
                        return;
                    case 28395871:
                        if (company.Amo_ids is null) company.Amo_ids = new();
                        //company.Amo_ids.ret_id = result.First().id;
                        return;
                    default:
                        return;
                }
        }

        public void Run()
        {
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(_request), _company); }
            catch (Exception e) { _log.Add($"Unable to process request: {e} --- Request: {_request}"); return; }

            #region Checking if already exist
            if (_company.Amo_ids is not null)
            {
                //UpdateCompanyInAmo(_corpRepo, FieldLists.CompanyCorp, _company, _company.Amo_ids.corp_id);
                return;
            }
            #endregion

            _1CRepo.UpdateCompany(_company);
        }
    }
}