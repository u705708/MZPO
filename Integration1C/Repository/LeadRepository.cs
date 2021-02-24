using Newtonsoft.Json;
using System.Net;

namespace Integration1C
{
    internal class LeadRepository
    {
        internal Lead GetLead(Lead lead) => GetLead(lead.lead_id_1C);

        internal Lead GetLead(int lead_id)
        {
            string uri = "";
            Request1C request = new("GET", uri);
            Lead result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Lead UpdateLead(Lead lead)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", uri, content);
            Lead result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Lead AddLead(Lead lead)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);
            Lead result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal bool AddToCourse(int lead_id)
        {
            string uri = "";
            Request1C request = new("GET", uri);
            Lead result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return true;
        }
    }
}
