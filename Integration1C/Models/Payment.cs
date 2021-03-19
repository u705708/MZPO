using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Integration1C
{
    public class Payment
    {
#pragma warning disable IDE1006 // Naming Styles
        [JsonConverter(typeof(DateFormatConverter))]
        public DateTime payment_date { get; set; }
        public int payment_amount { get; set; }
        public Guid client_id_1C { get; set; }
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