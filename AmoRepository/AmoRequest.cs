using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.AmoRepo
{
    internal class AmoRequest
    {
        #region Definition
        private readonly IAmoAuthProvider _auth;
        private readonly Uri _uri;
        private readonly HttpMethod _httpMethod;
        private readonly HttpContent _content;
        private readonly string content;

        internal AmoRequest(string httpMethod, string uri, string content, IAmoAuthProvider auth)
        {
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri(uri);
            _content = new StringContent(content);
            _auth = auth;
            this.content = content;
        }

        internal AmoRequest(string httpMethod, string uri, IAmoAuthProvider auth)
        {
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri(uri);
            _auth = auth;
        }
        #endregion

        #region Realization
        internal async Task<string> GetResponseAsync()
        {
            HttpResponseMessage response;
            
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(_httpMethod, _uri);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.GetToken());
            request.Headers.TryAddWithoutValidation("User-Agent", "mzpo2amo-client/1.1");
            
            if (_content is not null)
            {
                request.Content = _content;
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            var ss = _auth.GetSemaphoreSlim();

            await ss.WaitAsync();

            var getResponse = httpClient.SendAsync(request);

            await Task.WhenAll(
                Task.Delay(1000),
                getResponse);

            ss.Release();

            response = getResponse.Result;

            if (!response.IsSuccessStatusCode) throw new Exception($"Bad response: {response.Content.ReadAsStringAsync().Result} -- Request: {content}");
            return await response.Content.ReadAsStringAsync();
        }
        #endregion
    }
}