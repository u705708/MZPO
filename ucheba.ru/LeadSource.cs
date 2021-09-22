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
        public async static Task<LeadLite> GetLead(int id)
        {
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(new HttpMethod("GET"), new Uri($"https://api.ucheba.ru/v1/crm/institutions/9915/leads/{id}"));

            request.Headers.TryAddWithoutValidation("X-Auth-Token", await TokenProvider.GetAuthToken());

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Bad response: {response.Content.ReadAsStringAsync().Result}");

            LeadLite lead = new();

            string responseString = WebUtility.UrlDecode(await response.Content.ReadAsStringAsync());

            JsonConvert.PopulateObject(responseString, lead);

            return lead;
        }

        public static async Task<IEnumerable<LeadLite>> GetLeads(DateTime from, DateTime to)
        {
            string dates = $"?dateFrom={from.Year:d4}-{from.Month:d2}-{from.Day:d2}&dateTo={to.Year:d4}-{to.Month:d2}-{to.Day:d2}";

            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(new HttpMethod("GET"), new Uri($"https://api.ucheba.ru/v1/crm/institutions/9915/leads{dates}"));

            request.Headers.TryAddWithoutValidation("X-Auth-Token", await TokenProvider.GetAuthToken());

            var responseString = await httpClient.SendAsync(request);

            if (!responseString.IsSuccessStatusCode) throw new InvalidOperationException($"Bad response: {responseString.Content.ReadAsStringAsync().Result}");

            Response response = new();

            JsonConvert.PopulateObject(WebUtility.UrlDecode(await responseString.Content.ReadAsStringAsync()), response);

            if (response.items is null)
                return new List<LeadLite>();

            return new List<LeadLite>(response.items);
        }

        public static async Task<IEnumerable<LeadLite>> GetLeads() => await GetLeads(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));
    }
}