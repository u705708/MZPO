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

        internal Lead1C GetLead(Lead1C lead) => GetLead((Guid)lead.lead_id_1C);

        internal Lead1C GetLead(Guid lead_id)
        {
            string method = $"http://94.230.11.182:50080/uuc/hs/courses/EditApplication?id={lead_id:D}";
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

            string method = "http://94.230.11.182:50080/uuc/hs/courses/EditApplication";
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

            string method = "http://94.230.11.182:50080/uuc/hs/courses/EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}