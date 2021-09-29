using MZPO.ucheba.ru.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MZPO.ucheba.ru
{
    public static class LeadSource
    {
        public class UnauthorizedRequestException : InvalidOperationException
        {
            public UnauthorizedRequestException()
                : base()
            { }
        }

        private async static Task<T> GetAsync<T>(string uri, string token) where T : new()
        {
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(new HttpMethod("GET"), new Uri(uri));

            request.Headers.TryAddWithoutValidation("X-Auth-Token", token);

            var response = await httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new UnauthorizedRequestException();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Bad response: {response.Content.ReadAsStringAsync().Result}");

            T result = new();

            string responseString = WebUtility.UrlDecode(await response.Content.ReadAsStringAsync());

            JsonConvert.PopulateObject(responseString, result);

            return result;
        }

        public async static Task<LeadLite> GetLead(int id)
        {
            var uri = $"https://api.ucheba.ru/v1/crm/institutions/9915/leads/{id}";
            var token = await TokenProvider.GetAuthToken();

            try
            {
                return await GetAsync<LeadLite>(uri, token);
            }
            catch (UnauthorizedRequestException)
            {
                token = await TokenProvider.GetNewToken();
                return await GetAsync<LeadLite>(uri, token);
            }
        }

        public static async Task<IEnumerable<LeadLite>> GetLeads(DateTime from, DateTime to)
        {
            string dates = $"?dateFrom={from.Year:d4}-{from.Month:d2}-{from.Day:d2}&dateTo={to.Year:d4}-{to.Month:d2}-{to.Day:d2}";

            var uri = $"https://api.ucheba.ru/v1/crm/institutions/9915/leads{dates}";
            var token = await TokenProvider.GetAuthToken();

            try
            {
                var response = await GetAsync<Response>(uri, token);
                return new List<LeadLite>(response.items);
            }
            catch (UnauthorizedRequestException)
            {
                token = await TokenProvider.GetNewToken();
                var response = await GetAsync<Response>(uri, token);
                return new List<LeadLite>(response.items);
            }
        }

        public static async Task<IEnumerable<LeadLite>> GetLeads() => await GetLeads(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));
    }
}