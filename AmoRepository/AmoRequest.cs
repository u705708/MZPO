using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MZPO.AmoRepo
{
    internal class AmoRequest
    {
        public class TooManyRequestsException : InvalidOperationException
        {
            public TooManyRequestsException(string message)
                : base(message)
            { }
        }

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

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _auth.GetToken());
            request.Headers.TryAddWithoutValidation("User-Agent", "mzpo2amo-client/1.1");
            
            if (_content is not null)
            {
                request.Content = _content;
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            var ss = _auth.GetSemaphoreSlim();

            await ss.WaitAsync();

            var getResponse = Task.Run(async () => await httpClient.SendAsync(request));
            var ssRelease = Task.Run(async () => {
                await Task.Delay(1000);
                ss.Release();
            });
            await Task.WhenAny(getResponse, ssRelease);

            response = await getResponse;

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) throw new TooManyRequestsException($"ATTENTION!!! Request limit reached: {await response.Content.ReadAsStringAsync()}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await _auth.RefreshAmoAccountFromDBAsync();
                throw new InvalidOperationException($"Unathorized request: {await response.Content.ReadAsStringAsync()}"); 
            }
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Bad response: {await response.Content.ReadAsStringAsync()} -- Request: {content}");
            return await response.Content.ReadAsStringAsync();
        }
        #endregion
    }
}