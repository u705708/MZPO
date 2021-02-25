using MZPO.AmoRepo;
using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Update1CCompany
    {
        private readonly AmoAccount _acc;
        private readonly int _company_id;
        private readonly CompanyRepository _companyRepo1C;
        private readonly Log _log;

        public Update1CCompany(int company_id, AmoAccount acc, Log log)
        {
            _acc = acc;
            _company_id = company_id;
            _companyRepo1C = new();
            _log = log;
        }

        public void Run()
        {
            var compRepo = _acc.GetRepo<Company>();

            Dictionary<string, int> fieldIds;
            if (_acc.id == 19453687) fieldIds = FieldLists.CompanyCorp;
            else fieldIds = FieldLists.CompanyRet;

            var company = compRepo.GetById(_company_id);

            if (company is not null &&
                company.custom_fields_values is not null &&
                company.custom_fields_values.Any(x => x.field_id == fieldIds["company_id_1C"]))
                try { _companyRepo1C.UpdateCompany(Get1C.CompanyFromCompany(company, fieldIds)); }
                catch (Exception e) { _log.Add($"Unable to update company in 1C: {e}"); }
        }
    }
}