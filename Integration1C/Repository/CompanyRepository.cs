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

        private readonly Guid _mockGuid = new Guid("f4369528-14ba-4968-8a9c-6e8a3126378c");
        private readonly Company1C _mockCompany = new Company1C()
        {
            company_id_1C = new Guid("f4369528-14ba-4968-8a9c-6e8a3126378c"),
            amo_ids = new()
            {
                new()
                {
                    account_id = 19453687,
                    entity_id = 46776835
                }
            },
            name = "Тестовая компания",
            email = "test@email.com",
            phone = "+79001112233",
            signee = "Подписант",
            LPR_name = "Иван Иванов",
            OGRN = "13223131326",
            INN = "5465454654",
            acc_no = "4654654654654654",
            KPP = "546546465",
            BIK = "45654654",
            address = "ул. Пушкина, 10",
            post_address = "ул. Колотушкина, 11"
        };

        internal Company1C GetCompany(Company1C company) => GetCompany((Guid)company.company_id_1C);

        internal Company1C GetCompany(Guid company_id)
        {
            string method = $"EditPartner?id={company_id:D}";
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
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", method, content, _cred1C);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid AddCompany(Company1C company)
        {
            if (string.IsNullOrEmpty(company.email) &&
                string.IsNullOrEmpty(company.phone))
                throw new Exception("Unable to add company to 1C: no phone or email.");

            company.company_id_1C = null;

            string method = "EditPartner";
            string content = JsonConvert.SerializeObject(company, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}