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

        public class Result
        {
            public Guid lead_id_1C { get; set; }
        }

        internal Lead1C GetLead(Lead1C lead) => GetLead((Guid)lead.lead_id_1C);

        internal Lead1C GetLead(Guid lead_id)
        {
            string method = $"EditApplication?uid={lead_id:D}";
            Request1C request = new("GET", method, _cred1C);
            
            Lead1C result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result;
        }

        internal Guid UpdateLead(Lead1C lead)
        {
            if (lead.lead_id_1C is null ||
                lead.lead_id_1C == default)
                throw new Exception("Unable to update 1C lead, no UID.");

            string method = "EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result.lead_id_1C;
        }

        internal Guid AddLead(Lead1C lead)
        {
            if (lead.client_id_1C == default ||
                lead.product_id_1C == default)
                throw new Exception("Unable to update 1C lead, no linked entities.");

            lead.lead_id_1C = null;

            string method = "EditApplication";
            string content = JsonConvert.SerializeObject(lead, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result.lead_id_1C;
        }
    }
}