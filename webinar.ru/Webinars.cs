using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MZPO.webinar.ru
{
    public class Webinars
    {
        private readonly TokenProvider _tokenProvider;

        public Webinars()
        {
            _tokenProvider = new();
        }

        private static async Task<O> GetResult<O>(WebinarRequest request, O o)
        {
            try
            {
                var response = await request.GetResponseAsync();
                if (response == "") return o;
                JsonConvert.PopulateObject(WebUtility.UrlDecode(response), o);
            }
            catch (InvalidOperationException) { throw; }
            catch (Exception e) { throw new ArgumentException("Unable to process response from webinar.ru: " + e.Message); }
            return o;
        }

        public async Task<List<TimeZone>> GetTimeZones()
        {
            var uri = "https://userapi.webinar.ru/v3/timezones";
            List<TimeZone> result = new();
            WebinarRequest request = new("GET", uri, _tokenProvider);
            return await GetResult(request, result);
        }

        public async Task<Event> GetEvent(long eventID)
        {
            var uri = $"https://userapi.webinar.ru/v3/organization/events/{eventID}";
            Event result = new();
            WebinarRequest request = new("GET", uri, _tokenProvider);
            return await GetResult(request, result);
        }

        public async Task<EventSession> GetEventSession(long eventSessionID)
        {
            var uri = $"https://userapi.webinar.ru/v3/organization/eventsessions/{eventSessionID}";
            EventSession result = new();
            WebinarRequest request = new("GET", uri, _tokenProvider);
            return await GetResult(request, result);
        }

        public async Task<List<Event>> GetSchedule()
        {
            var uri = $"https://userapi.webinar.ru/v3/organization/events/schedule";
            List<Event> result = new();
            WebinarRequest request = new("GET", uri, _tokenProvider);
            return await GetResult(request, result);
        }

        public async Task<List<Participant>> GetParticipants(long eventSessionID)
        {
            var uri = $"https://userapi.webinar.ru/v3/eventsessions/{eventSessionID}/participations?perPage=500";
            List<Participant> result = new();
            WebinarRequest request = new("GET", uri, _tokenProvider);
            return await GetResult(request, result);
        }

        public async Task<List<User>> SearchUser(string email)
        {
            var uri = $"https://userapi.webinar.ru/v3/contacts/search?contactsData[email]={email}";
            List<User> result = new();
            WebinarRequest request = new("GET", uri, _tokenProvider);
            return await GetResult(request, result);
        }

        public async Task<User> GetUser(long contactsID)
        {
            var uri = $"https://userapi.webinar.ru/v3/contacts/{contactsID}";
            User result = new();
            WebinarRequest request = new("GET", uri, _tokenProvider);
            return await GetResult(request, result);
        }

        public async Task<RegisterResponse> AddUserToEventSession(long eventSessionID, User payload)
        {
            var uri = $"https://userapi.webinar.ru/v3/eventsessions/{eventSessionID}/register";
            RegisterResponse result = new();
            string content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            WebinarRequest request = new("POST", uri, content, _tokenProvider);
            return await GetResult(request, result);
        }
        public async Task<RegisterResponse> AddUserToEventSession(long eventSessionID, string email) => await AddUserToEventSession(eventSessionID, new User() { email = email });

        public async Task<RegisterResponse> AddUserToEvent(long eventID, User payload)
        {
            var uri = $"https://userapi.webinar.ru/v3/events/{eventID}/register";
            RegisterResponse result = new();
            string content = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            WebinarRequest request = new("POST", uri, content, _tokenProvider);
            return await GetResult(request, result);
        }
        public async Task<RegisterResponse> AddUserToEvent(long eventID, string email) => await AddUserToEvent(eventID, new User() { email = email });

    }
}