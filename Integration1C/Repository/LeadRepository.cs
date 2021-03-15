using Newtonsoft.Json;
using System;
using System.Net;

namespace Integration1C
{
    internal class LeadRepository
    {
        internal Lead1C GetLead(Lead1C lead) => GetLead(lead.lead_id_1C);

        internal Lead1C GetLead(Guid lead_id)
        {
            string uri = $"http://94.230.11.182:50080/uuc/hs/courses/EditApplication?id={lead_id.ToString("D")}";
            Request1C request = new("GET", uri);
            
            Lead1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Lead1C UpdateLead(Lead1C lead)
        {
            string uri = "http://94.230.11.182:50080/uuc/hs/courses/EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", uri, content);
            
            Lead1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Lead1C AddLead(Lead1C lead)
        {
            string uri = "http://94.230.11.182:50080/uuc/hs/courses/EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);
            
            Lead1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}