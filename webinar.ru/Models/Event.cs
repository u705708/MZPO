using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace MZPO.webinar.ru
{
    public class Event
    {
#pragma warning disable IDE1006 // Naming Styles
        public long id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public int? access { get; set; }
        public string lang { get; set; }
        public DateTime startsAt { get; set; }
        public long utcStartsAt { get; set; }
        public long? createUserId { get; set; }
        public int? timezoneId { get; set; }
        public TimeZone timezone { get; set; }
        public DateTime endsAt { get; set; }
        public string duration { get; set; }
        public long? organizationId { get; set; }
        public string type { get; set; }
        public User createUser { get; set; }
        public string rule { get; set; }
        public List<AdditionalField> additionalFields { get; set; }
        public List<EventSession> eventSessions { get; set; }
        public string description { get; set; }
        public bool? isArchive { get; set; }
        public bool? isAuto { get; set; }
#pragma warning restore IDE1006 // Naming Styles    
    }
}