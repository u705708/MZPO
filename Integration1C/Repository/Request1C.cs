using MZPO.Services;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Integration1C
{
    internal class Request1C
    {
        #region Definition
        private readonly Uri _uri;
        private readonly HttpMethod _httpMethod;
        private readonly HttpContent _content;
        private readonly string content;
        private readonly Credentials1C _credentials1C;

        internal Request1C(string httpMethod, string method, string content, Cred1C cred1C)
        {
            _credentials1C = cred1C.GetCredentials();
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri($"{_credentials1C.uri}{method}");
            _content = new StringContent(content);
            this.content = content;
        }

        internal Request1C(string httpMethod, string method, Cred1C cred1C)
        {
            _credentials1C = cred1C.GetCredentials();
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri($"{_credentials1C.uri}{method}");
        }
        #endregion

        #region Realization
        internal string GetResponse()
        {
            HttpResponseMessage response;

            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(_httpMethod, _uri);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                                    "Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_credentials1C.username}:{_credentials1C.pwd}")));
            request.Headers.TryAddWithoutValidation("User-Agent", "mzpo1C-client/1.0");

            if (_content is not null)
            {
                request.Content = _content;
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            response = httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode) throw new Exception($"Bad response: {response.StatusCode} -- Request: {content}");
            return response.Content.ReadAsStringAsync().Result;
        }
        #endregion
    }
}