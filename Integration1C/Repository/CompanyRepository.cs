using Newtonsoft.Json;
using System.Net;

namespace Integration1C
{
    internal class CompanyRepository
    {
        internal Company GetCompany(Company company) => GetCompany(company.company_id_1C);

        internal Company GetCompany(int company_id)
        {
            string uri = "";
            Request1C request = new("GET", uri);
            Company result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Company UpdateCompany(Company company)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", uri, content);
            Company result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Company AddCompany(Company company)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);
            Company result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}