using MZPO.AmoRepo;
using MZPO.DBRepository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace MZPO.Services
{
    public class AuthProvider : IAmoAuthProvider
    {
        #region Definition

        private const int _concurrentRequestsAmount = 6;

        private readonly AmoProvider _amoProvider;
        private AmoAccountAuth _amoAccountAuth;
        private string _authToken;
        private string _refrToken;
        private DateTime _validity;
        private readonly SemaphoreSlim _ss;

        public AuthProvider(AmoAccountAuth acc, AmoProvider prov)
        {
            _amoAccountAuth = acc;
            _amoProvider = prov;
            _authToken = acc.authToken;
            _refrToken = acc.refrToken;
            _validity = acc.validity;
            _ss = new(_concurrentRequestsAmount, _concurrentRequestsAmount);
        }
        #endregion

        #region Supplementary methods
        private bool IsValid()
        {
            return _validity > DateTime.Now;
        }

        private void Refresh()
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

                ProcessResponse(GetResponse(content));

                _amoProvider.UpdateAccount(_amoAccountAuth);
            }
            catch
            {
                GetNew();
            }
        }

        private void GetNew()
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

                ProcessResponse(GetResponse(content));

                _amoProvider.UpdateAccount(_amoAccountAuth);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to update token: {e.Message}");
            }
        }

        private string GetResponse(string payload)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"https://{_amoAccountAuth.subdomain}.amocrm.ru/oauth2/access_token");
            request.Headers.TryAddWithoutValidation("User-Agent", "amoCRM-oAuth-client/1.0");
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            try
            {
                HttpResponseMessage responseMessage = httpClient.SendAsync(request).Result;
                return responseMessage.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                throw new Exception("Can't get token: " + e.Message);
            }
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
                _validity = DateTime.Now.AddSeconds((int)securityData["expires_in"] - 5);
                _amoAccountAuth.validity = _validity;
            }
            catch (Exception e)
            {
                throw new Exception("Unable to update token: " + e.Message);
            }
        }
        #endregion

        #region Realization
        public string GetToken()
        {
            if (!IsValid()) Refresh();
            return _authToken;
        }

        public int GetAccountId()
        {
            return _amoAccountAuth.id;
        }

        public SemaphoreSlim GetSemaphoreSlim()
        {
            return _ss;
        }
        #endregion
    }
}