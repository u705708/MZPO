using System;
using System.Collections.Generic;

namespace MZPO.webinar.ru
{
    public class Participant
    {
#pragma warning disable IDE1006 // Naming Styles
        public long id { get; set; }
        public string name { get; set; }
        public string secondName { get; set; }
        public string email { get; set; }
        public long eventId { get; set; }
        public long eventSessionId { get; set; }
        public int isAccepted { get; set; }
        public string role { get; set; }
        public int isSeen { get; set; }
        public string agreementStatus { get; set; }
        public string referrer { get; set; }
        public long userId { get; set; }
        public int isOnline { get; set; }
        public string registerStatus { get; set; }
        public DateTime? registerDate { get; set; }
        public string paymentStatus { get; set; }
        public string url { get; set; }
        public List<AdditionalField> additionalFieldValues { get; set; }
        public bool visited { get; set; }
#pragma warning restore IDE1006 // Naming Styles    
    }
}