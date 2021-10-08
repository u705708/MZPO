using MZPO.AmoRepo;
using MZPO.DBRepository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MZPO.Services
{
    public class AuthProvider : IAmoAuthProvider
    {
        #region Definition

        private readonly int _concurrentRequestsAmount;

        private readonly AmoProvider _amoProvider;
        private readonly SemaphoreSlim _amoConnectionsSemaphore;
        private readonly SemaphoreSlim _tokenUpdateSemaphore;
        private AmoAccountAuth _amoAccountAuth;
        private string _authToken;
        private string _refrToken;
        private DateTime _validity;

        public AuthProvider(AmoAccountAuth acc, AmoProvider prov, int concurrentConnections)
        {
            _concurrentRequestsAmount = concurrentConnections;
            _amoProvider = prov;
            _amoConnectionsSemaphore = new(_concurrentRequestsAmount, _concurrentRequestsAmount);
            _tokenUpdateSemaphore = new(1, 1);
            _amoAccountAuth = acc;
            _authToken = acc.authToken;
            _refrToken = acc.refrToken;
            _validity = acc.validity;
        }
        #endregion

        #region Supplementary methods
        private bool IsValid()
        {
            return _validity > DateTime.UtcNow;
        }

        private async Task Refresh()
        {
            try
            {
                var content = JsonConvert.SerializeObject(new
                {
                    _amoAccountAuth.client_id,
                    _amoAccountAuth.client_secret,
                    grant_type = "refresh_token",
                    refresh_token = _refrToken,
                    _amoAccountAuth.redirect_uri
                },
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                ProcessResponse(await GetResponse(content));

                await _amoProvider.UpdateAccountAsync(_amoAccountAuth);
            }
            catch (ArgumentException) { await GetNew(); }
            catch (InvalidOperationException) { await GetNew(); }
            catch (Exception e){ throw new InvalidOperationException($"Unable to refresh token: {e.Message}"); }
        }

        private async Task GetNew()
        {
            try
            {
                var content = JsonConvert.SerializeObject(new
                        {
                            _amoAccountAuth.client_id,
                            _amoAccountAuth.client_secret,
                            grant_type = "authorization_code",
                            _amoAccountAuth.code,
                            _amoAccountAuth.redirect_uri
                        },
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                ProcessResponse(await GetResponse(content));

                await _amoProvider.UpdateAccountAsync(_amoAccountAuth);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to get new token: {e.Message}");
            }
        }

        private async Task<string> GetResponse(string payload)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"https://{_amoAccountAuth.subdomain}.amocrm.ru/oauth2/access_token");
            request.Headers.TryAddWithoutValidation("User-Agent", "amoCRM-oAuth-client/1.0");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            await _amoConnectionsSemaphore.WaitAsync();

            var getResponse = Task.Run(async () => await httpClient.SendAsync(request));
            var ssRelease = Task.Run(async () => {
                await Task.Delay(1000);
                _amoConnectionsSemaphore.Release();
            });
            await Task.WhenAny(getResponse, ssRelease);

            var response = await getResponse;

            if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Incorrect response: {await response.Content.ReadAsStringAsync()}");

            return await response.Content.ReadAsStringAsync();
        }

        private void ProcessResponse(string response)
        {
            try
            {
                JObject securityData = JObject.Parse(response);
                _authToken = (string)securityData["access_token"];
                _amoAccountAuth.authToken = _authToken;
                _refrToken = (string)securityData["refresh_token"];
                _amoAccountAuth.refrToken = _refrToken;
                _validity = DateTime.UtcNow.AddSeconds((int)securityData["expires_in"] - 5);
                _amoAccountAuth.validity = _validity;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Unable to process response: " + e.Message);
            }
        }
        #endregion

        #region Realization
        public async Task<string> GetToken()
        {
            await _tokenUpdateSemaphore.WaitAsync();
            if (!IsValid()) await Refresh();
            _tokenUpdateSemaphore.Release();
            return _authToken;
        }

        public int GetAccountId()
        {
            return _amoAccountAuth.id;
        }

        public SemaphoreSlim GetSemaphoreSlim()
        {
            return _amoConnectionsSemaphore;
        }

        public async Task RefreshAmoAccountFromDBAsync()
        {
            var acc = await _amoProvider.GetAmoAccountAsync(_amoAccountAuth.id);
            _amoAccountAuth = acc;
            _authToken = acc.authToken;
            _refrToken = acc.refrToken;
            _validity = acc.validity;
        }
        #endregion
    }
}