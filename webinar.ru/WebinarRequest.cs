using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MZPO.webinar.ru
{
    internal class WebinarRequest
    {
        public class TooManyRequestsException : InvalidOperationException
        {
            public TooManyRequestsException(string message)
                : base(message)
            { }
        }

        #region Definition
        private readonly TokenProvider _tokenProvider;
        private readonly Uri _uri;
        private readonly HttpMethod _httpMethod;
        private readonly HttpContent _content;
        private readonly string content;

        internal WebinarRequest(string httpMethod, string uri, string content, TokenProvider tokenProvider)
        {
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri(uri);
            _content = new StringContent(content);
            _tokenProvider = tokenProvider;
            this.content = content;
        }

        internal WebinarRequest(string httpMethod, string uri, TokenProvider tokenProvider)
        {
            _httpMethod = new HttpMethod(httpMethod);
            _uri = new Uri(uri);
            _tokenProvider = tokenProvider;
        }
        #endregion

        #region Realization
        internal async Task<string> GetResponseAsync()
        {
            HttpResponseMessage response;

            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(_httpMethod, _uri);

            request.Headers.TryAddWithoutValidation("X-Auth-Token", await _tokenProvider.GetToken());

            if (_content is not null)
            {
                request.Content = _content;
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            var getResponse = Task.Run(async () => await httpClient.SendAsync(request));
            var ssRelease = Task.Run(async () => {
                await Task.Delay(1000);
                _tokenProvider.ReleaseToken();
            });
            await Task.WhenAny(getResponse, ssRelease);

            response = await getResponse;

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) throw new TooManyRequestsException($"ATTENTION!!! Request limit reached: {await response.Content.ReadAsStringAsync()}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) throw new InvalidOperationException($"Unathorized request: {await response.Content.ReadAsStringAsync()}");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Bad response: {await response.Content.ReadAsStringAsync()} -- Request: {content}");
            return await response.Content.ReadAsStringAsync();
        }
        #endregion
    }
}
