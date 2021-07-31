using MZPO.Services;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Integration1C
{
    internal class ClientRepository
    {
        private readonly Cred1C _cred1C;

        public ClientRepository(Cred1C cred1C)
        {
            _cred1C = cred1C;
        }

        public class Result
        {
            public Guid client_id_1C { get; set; }
        }

        internal Client1C GetClient(Client1C client) => GetClient((Guid)client.client_id_1C);

        internal Client1C GetClient(Guid client_id)
        {
            string method = $"EditStudent?uid={client_id:D}";
            Request1C request = new("GET", method, _cred1C);

            Client1C result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result;
        }

        internal Guid UpdateClient(Client1C client)
        {
            if (client.client_id_1C is null ||
                client.client_id_1C == default)
                throw new Exception("Unable to update 1C client, no UID.");

            string method = "EditStudent";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result.client_id_1C;
        }

        internal Guid AddClient(Client1C client)
        {
            if (string.IsNullOrEmpty(client.email) &&
                string.IsNullOrEmpty(client.phone))
                throw new Exception("Unable to add client to 1C: no phone or email.");

            client.client_id_1C = null;

            string method = "EditStudent";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include }); ;
            Request1C request = new("POST", method, content, _cred1C);

            Result result = new();
            var response = request.GetResponse();
            try { JsonConvert.PopulateObject(WebUtility.UrlDecode(response), result); }
            catch (Exception e) { throw new Exception($"Unable to process response from 1C: {e.Message}, Response: {response}"); }
            return result.client_id_1C;
        }
    }
}