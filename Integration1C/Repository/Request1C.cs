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

        internal Request1C(string httpMethod, string uri, string content)
        {
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri(uri);
            _content = new StringContent(content);
            this.content = content;
        }

        internal Request1C(string httpMethod, string uri)
        {
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri(uri);
        }
        #endregion

        #region Realization
        internal string GetResponse()
        {
            HttpResponseMessage response;

            using HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(_httpMethod, _uri);

            request.Headers.TryAddWithoutValidation("User-Agent", "mzpo1C-client/1.0");

            if (_content is not null)
            {
                request.Content = _content;
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            response = httpClient.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode) throw new Exception($"Bad response: {response.Content.ReadAsStringAsync().Result} -- Request: {content}");
            return response.Content.ReadAsStringAsync().Result;
        }
        #endregion
    }
}