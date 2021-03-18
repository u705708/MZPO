using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.LeadProcessors
{
    public class FormRequest
    {
#pragma warning disable IDE1006 // Naming Styles
        public string name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string comment { get; set; }
        public string form_name_site { get; set; }
        public string site { get; set; }
        public string page_url { get; set; }
        public string page_title { get; set; }
        public string roistat_marker { get; set; }
        public string roistat_visit { get; set; }
        public string city_name { get; set; }
        public string status { get; set; }
        public string pipeline { get; set; }
        public string _ym_uid { get; set; }
        public string _ya_uid { get; set; }
        public string clid { get; set; }
        public string utm_source { get; set; }
        public string utm_medium { get; set; }
        public string utm_term { get; set; }
        public string utm_content { get; set; }
        public string utm_campaign { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}