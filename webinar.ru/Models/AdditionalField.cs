using System.Collections.Generic;

namespace MZPO.webinar.ru
{
    public class AdditionalField
    {
#pragma warning disable IDE1006 // Naming Styles
        public string key { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public bool? isRequired { get; set; }
        public string placeholder { get; set; }
        public List<string> values { get; set; }
        public string value { get; set; }
#pragma warning restore IDE1006 // Naming Styles    
    }
}