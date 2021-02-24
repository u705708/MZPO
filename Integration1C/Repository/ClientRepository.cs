using Newtonsoft.Json;
using System.Net;

namespace Integration1C
{
    internal class ClientRepository
    {
        internal Client GetClient(Client client) => GetClient(client.client_id_1C);

        internal Client GetClient(int client_id)
        {
            string uri = "";
            Request1C request = new("GET", uri);
            Client result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Client UpdateClient(Client client)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("PATCH", uri, content);
            Client result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }

        internal Client AddClient(Client client)
        {
            string uri = "";
            string content = JsonConvert.SerializeObject(client, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); ;
            Request1C request = new("POST", uri, content);
            Client result = new();
            JsonConvert.PopulateObject(WebUtility.UrlDecode(request.GetResponse()), result);
            return result;
        }
    }
}
