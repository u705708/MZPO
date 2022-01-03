namespace MZPO.webinar.ru
{
    public class TimeZone
    {
#pragma warning disable IDE1006 // Naming Styles
        public int id { get; set; }
        public bool isDeleted { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string shortDescription { get; set; }
        public string abbreviation { get; set; }
        public int offset { get; set; }
#pragma warning restore IDE1006 // Naming Styles    
    }
}