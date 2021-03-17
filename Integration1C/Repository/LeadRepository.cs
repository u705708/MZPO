using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Integration1C
{
    internal class LeadRepository
    {
        internal Lead1C GetLead(Lead1C lead) => GetLead((Guid)lead.lead_id_1C);

        internal Lead1C GetLead(Guid lead_id)
        {
            string uri = $"http://94.230.11.182:50080/uuc/hs/courses/EditApplication?id={lead_id:D}";
            Request1C request = new("GET", uri);
            
            Lead1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Guid UpdateLead(Lead1C lead)
        {
            if (lead.lead_id_1C is null ||
                lead.lead_id_1C == default)
                throw new Exception("Unable to update 1C lead, no UID.");

            string uri = "http://94.230.11.182:50080/uuc/hs/courses/EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", uri, content);

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

            string uri = "http://94.230.11.182:50080/uuc/hs/courses/EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);

            Guid result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}