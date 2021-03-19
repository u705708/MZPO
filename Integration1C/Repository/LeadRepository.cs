using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Integration1C
{
    internal class LeadRepository
    {
        private readonly Cred1C _cred1C;

        public LeadRepository(Cred1C cred1C)
        {
            _cred1C = cred1C;
        }

        private readonly Guid _mockGuid = new Guid("628c57d5-3338-4366-9691-942774e8323f");
        private readonly Lead1C _mockLeadCorp = new Lead1C()
        {
            lead_id_1C = new Guid("628c57d5-3338-4366-9691-942774e8323f"),
            amo_ids = new()
            {
                new()
                {
                    account_id = 19453687,
                    entity_id = 1795667
                }
            },
            client_id_1C = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40"),
            product_id_1C = new Guid("1205f8a9-0a5a-47d1-99e2-30a2d2823948"),
            company_id_1C = new Guid("f4369528-14ba-4968-8a9c-6e8a3126378c"),
            organization = "МЦПО",
            price = 10000,
            is_corporate = true,
            lead_status = "",
            marketing_channel = "",
            marketing_source = "",
            author = "",
            responsible_user = "",
            payments = new()
            {
                new()
                {
                    payment_date = DateTime.Now,
                    payment_amount = 5000,
                    client_id_1C = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40")
                }
            }
        };
        private readonly Lead1C _mockLeadRet = new Lead1C()
        {
            lead_id_1C = new Guid("628c57d5-3338-4366-9691-942774e8323f"),
            amo_ids = new()
            {
                new()
                {
                    account_id = 28395871,
                    entity_id = 23860689
                }
            },
            client_id_1C = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40"),
            product_id_1C = new Guid("1205f8a9-0a5a-47d1-99e2-30a2d2823948"),
            company_id_1C = new Guid("f4369528-14ba-4968-8a9c-6e8a3126378c"),
            organization = "МЦПО",
            price = 10000,
            is_corporate = false,
            lead_status = "",
            marketing_channel = "",
            marketing_source = "",
            author = "",
            responsible_user = "",
            payments = new()
            {
                new()
                {
                    payment_date = DateTime.Now,
                    payment_amount = 5000,
                    client_id_1C = new Guid("2da8d74a-a672-45b8-8f90-cfd076392b40")
                }
            }
        };

        internal Lead1C GetLead(Lead1C lead) => GetLead((Guid)lead.lead_id_1C);

        internal Lead1C GetLead(Guid lead_id)
        {
            string method = $"EditApplication?id={lead_id:D}";
            Request1C request = new("GET", method, _cred1C);
            
            Lead1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid UpdateLead(Lead1C lead)
        {
            if (lead.lead_id_1C is null ||
                lead.lead_id_1C == default)
                throw new Exception("Unable to update 1C lead, no UID.");

            string method = "EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", method, content, _cred1C);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid AddLead(Lead1C lead)
        {
            if (lead.client_id_1C == default ||
                lead.product_id_1C == default)
                throw new Exception("Unable to update 1C lead, no linked entities.");

            lead.lead_id_1C = null;

            string method = "EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}