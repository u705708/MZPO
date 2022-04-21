namespace MZPO.LeadProcessors
{
    public class CreateLKResponse
    {
        public string[] email { get; set; }
        public string[] phone { get; set; }
        public string id { get; set; }
        public bool? result { get; set; }
        public int? user { get; set; }
        public string token { get; set; }
    }
}