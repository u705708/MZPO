using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Integration1C
{
    internal class CompanyRepository
    {
        private readonly Cred1C _cred1C;

        public CompanyRepository(Cred1C cred1C)
        {
            _cred1C = cred1C;
        }

        public class Result
        {
            public Guid company_id_1C { get; set; }
        }

        internal Company1C GetCompany(Company1C company) => GetCompany((Guid)company.company_id_1C);

        internal Company1C GetCompany(Guid company_id)
        {
            string method = $"EditPartner?uid={company_id:D}";
            Request1C request = new("GET", method, _cred1C);
            
            Company1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid UpdateCompany(Company1C company)
        {
            if (company.company_id_1C is null ||
                company.company_id_1C == default)
                throw new Exception("Unable to update 1C client, no UID.");

            string method = "EditPartner";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result); }
            catch (Exception e) { return default; }
            return result.company_id_1C;
        }

        internal Guid AddCompany(Company1C company)
        {
            if (string.IsNullOrEmpty(company.email) &&
                string.IsNullOrEmpty(company.phone) &&
                string.IsNullOrEmpty(company.name) &&
                string.IsNullOrEmpty(company.INN))
                throw new Exception("Unable to add company to 1C: no phone or email.");

            company.company_id_1C = null;

            string method = "EditPartner";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result); }
            catch (Exception e) { return default; }
            return result.company_id_1C;
        }
    }
}