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
    public class MZPOLK
    {
        #region Definition
        private readonly Uri _uri;
        private readonly HttpMethod _httpMethod;
        private readonly HttpContent _content;

        public MZPOLK(CreateLKRequest request)
        {
            _httpMethod = HttpMethod.Post;
            _uri = new Uri("https://lk.mzpo-s.ru/amo/register");
            _content = new StringContent(JsonConvert.SerializeObject(request, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
        #endregion

        #region Realization
        public async Task<CreateLKResponse> CreateLKAsync()
        {
            HttpResponseMessage response;

            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(_httpMethod, _uri);

            request.Content = _content;
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound) throw new InvalidOperationException($"Bad response: {await response.Content.ReadAsStringAsync()} -- Request: {await _content.ReadAsStringAsync()}");

            CreateLKResponse result = new();

            string data = await response.Content.ReadAsStringAsync();

            try
            {
                JsonConvert.PopulateObject(data, result);
            }
            catch
            {
                throw new InvalidOperationException($"Create LK API returned invalid result. Request: {await _content.ReadAsStringAsync()}. Reponse: {data}");
            }

            return result;
        }
        #endregion
    }
}