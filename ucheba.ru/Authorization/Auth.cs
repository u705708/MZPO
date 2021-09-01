using MZPO.ucheba.ru.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MZPO.ucheba.ru
{
    internal static class Auth
    {
        private static string GetResponse(string payload)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri("https://api.ucheba.ru/v1/auth"));
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            try
            {
                var responseMessage = httpClient.SendAsync(request);
                return responseMessage.Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                throw new Exception("Can't get ucheba.ru token: " + e.Message);
            }
        }

        private static Token ProcessResponse(string response)
        {
            try
            {
                return JsonConvert.DeserializeObject<Token>(response);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to process ucheba.ru token: " + e.Message);
            }
        }

        internal static Token GetNewToken()
        {
            string payload = JsonConvert.SerializeObject(CredentialsProvider.GetCredentials(), Formatting.Indented);
            return ProcessResponse(GetResponse(payload));
        }
    }
}