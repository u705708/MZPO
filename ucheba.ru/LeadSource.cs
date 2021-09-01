using MZPO.ucheba.ru.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MZPO.ucheba.ru
{
    public static class LeadSource
    {
        public static IEnumerable<Lead> GetLeads(DateTime from, DateTime to)
        {
            string dates = $"?dateFrom={from.Year}-{from.Month}-{from.Day}&dateTo={to.Year}-{to.Month}-{to.Day}";

            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(new HttpMethod("GET"), new Uri($"https://api.ucheba.ru/v1/crm/institutions/??/leads{dates}"));

            request.Headers.TryAddWithoutValidation("X-Auth-Token", TokenProvider.GetAuthToken());

            var responseString = httpClient.SendAsync(request).Result;

            if (!responseString.IsSuccessStatusCode) throw new Exception($"Bad response: {responseString.Content.ReadAsStringAsync().Result}");

            Response response = new();

            JsonConvert.PopulateObject(WebUtility.UrlDecode(responseString.Content.ReadAsStringAsync().Result), response);

            if (response.items is not null &&
                response.items.Any())
                foreach (var i in response.items)
                    yield return i;

            yield break;
        }

        public static IEnumerable<Lead> GetLeads()
        {
            var now = DateTime.Now;
            var today = new DateTime(now.Year, now.Month, now.Day);
            return GetLeads(today.AddDays(-1), today);
        }

        public static Lead GetLead(int id)
        {
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(new HttpMethod("GET"), new Uri($"https://api.ucheba.ru/v1/crm/institutions/??/leads/{id}"));

            request.Headers.TryAddWithoutValidation("X-Auth-Token", TokenProvider.GetAuthToken());

            var responseString = httpClient.SendAsync(request).Result;

            if (!responseString.IsSuccessStatusCode) throw new Exception($"Bad response: {responseString.Content.ReadAsStringAsync().Result}");

            Lead lead = new();

            JsonConvert.PopulateObject(WebUtility.UrlDecode(responseString.Content.ReadAsStringAsync().Result), lead);

            return lead;
        }
    }
}