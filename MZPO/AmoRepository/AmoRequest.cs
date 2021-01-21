using MZPO.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MZPO.AmoRepo
{
    public class AmoRequest
    {
        #region Definition
        private readonly AuthProvider _auth;
        private readonly Uri _uri;
        private readonly HttpMethod _httpMethod;
        private readonly HttpContent _content;
        private readonly string content;

        public AmoRequest(string httpMethod, string uri, string content, AuthProvider auth)
        {
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri(uri);
            _content = new StringContent(content);
            _auth = auth;
            this.content = content;
        }

        public AmoRequest(string httpMethod, string uri, AuthProvider auth)
        {
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri(uri);
            _auth = auth;
        }
        #endregion

        #region Realization
        public string GetResponse()
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using HttpRequestMessage request = new HttpRequestMessage(_httpMethod, _uri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.GetToken());
                request.Headers.TryAddWithoutValidation("User-Agent", "mzpo2amo-client/1.1");
                if (!(_content == null))
                {
                    request.Content = _content;
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                }
                response = httpClient.SendAsync(request).Result;
            }
            if (!response.IsSuccessStatusCode) throw new Exception($"Bad response: {response.Content.ReadAsStringAsync().Result} -- Request: {content}");
            else return response.Content.ReadAsStringAsync().Result;
        }
        #endregion
    }
}
