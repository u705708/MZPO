using MZPO.ucheba.ru.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MZPO.ucheba.ru
{
    internal static class Auth
    {
        internal static async Task<Token> GetNewToken()
        {
            string payload = JsonConvert.SerializeObject(await CredentialsProvider.GetCredentials(), Formatting.Indented);
            return ProcessResponse(await GetResponse(payload));
        }

        private static async Task<string> GetResponse(string payload)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), new Uri("https://api.ucheba.ru/v1/auth"));
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            try
            {
                var responseMessage = await httpClient.SendAsync(request);
                return await responseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Can't get ucheba.ru token: " + e.Message);
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
                throw new ArgumentException("Unable to process ucheba.ru token: " + e.Message);
            }
        }
    }
}