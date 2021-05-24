using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.AmoRepo
{
    public class Custom_fields_value
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// ID поля.
        /// </summary>
        public int field_id { get; set; }
        /// <summary>
        /// Название поля.
        /// </summary>
        public string field_name { get; set; }
        /// <summary>
        /// Код поля.
        /// </summary>
        public string field_code { get; set; }
        /// <summary>
        /// Тип поля.
        /// </summary>
        public string field_type { get; set; }
        /// <summary>
        /// Массив значений поля
        /// </summary>
        public Values[] values { get; set; }

        public class Values
        {
            /// <summary>
            /// Значения поля.
            /// </summary>
            [JsonProperty(NullValueHandling = NullValueHandling.Include)]
            public object value { get; set; }
            public int? enum_id { get; set; }
            public string enum_code { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
