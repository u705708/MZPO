using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class MZPOEvent
    {
        #region Definition
        private readonly Uri _uri;
        private readonly HttpMethod _httpMethod;
        private readonly HttpContent _content;
        private readonly string _event_name;

        public MZPOEvent(string event_name)
        {
            _httpMethod = HttpMethod.Post;
            _uri = new Uri("https://www.mzpo-s.ru/api/activities_api.php");
            _content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mzpo2amo", "xDvkV@DgpsWh"),
                new KeyValuePair<string, string>("secret", "i$eSem64nQka"),
                new KeyValuePair<string, string>("name", System.Web.HttpUtility.HtmlEncode(event_name)),

            });
            _event_name = event_name;
        }
        #endregion

        #region Realization
        public async Task<EventProperties> GetPropertiesAsync()
        {
            HttpResponseMessage response;

            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(_httpMethod, _uri);

            request.Content = _content;
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Bad response: {await response.Content.ReadAsStringAsync()} -- Request: {await _content.ReadAsStringAsync()}");

            EventProperties result = new();

            try
            {
                string data = await response.Content.ReadAsStringAsync();

                JsonConvert.PopulateObject(data, result);
            }
            catch
            {
                throw new InvalidOperationException($"Activities API returned no results for {_event_name}");
            }

            return result;
        }
        #endregion
    }
}