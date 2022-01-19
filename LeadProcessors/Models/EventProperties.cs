using System;

namespace MZPO.LeadProcessors
{
    public class EventProperties
    {
#pragma warning disable IDE1006 // Naming Styles
        public int id { get; set; }
        public string page_name { get; set; }
        public DateTime vrema { get; set; }
        public string event_address { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}