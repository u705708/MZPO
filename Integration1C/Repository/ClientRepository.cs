using Newtonsoft.Json;
using System.Net;

namespace Integration1C
{
    internal class ClientRepository
    {
        internal Client1C GetClient(Client1C client) => GetClient(client.client_id_1C);

        internal Client1C GetClient(int client_id)
        {
            string uri = "";
            Request1C request = new("GET", uri);
            Client1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Client1C UpdateClient(Client1C client)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", uri, content);
            Client1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Client1C AddClient(Client1C client)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);
            Client1C result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}
