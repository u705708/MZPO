using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Integration1C
{
    public class Client1C
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid? client_id_1C { get; set; }
        public List<Amo_id> amo_ids { get; set; }

        private string _email;
        public string email {
            get
            { return _email; }
            set
            { _email = value.Trim().Replace(" ", ""); }
        }

        private string _phone;
        public string phone {
            get
            {
                if (_phone.StartsWith("89"))
                    return $"7{_phone[1..]}";
                return _phone; 
            }
            set
            { _phone = value.Trim().Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", ""); }
        }

        public string name { get; set; }
        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime? dob { get; set; }
        public string snils { get; set; }
        public string pass_serie { get; set; }
        public string pass_number { get; set; }
        public string pass_issued_by { get; set; }
        public string pass_dpt_code { get; set; }

        private DateTime _pass_issued_at;
        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime pass_issued_at {
            get
            {
                if (_pass_issued_at < new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return _pass_issued_at;
            }
            set
            {
                _pass_issued_at = value < DateTime.UnixEpoch ? DateTime.UnixEpoch : value;
            }
        }
#pragma warning restore IDE1006 // Naming Styles    

        public class DateFormatConverter : IsoDateTimeConverter
        {
            public DateFormatConverter()
            {
                DateTimeFormat = "dd.MM.yyyy H:mm:ss";
            }
        }
    }
}