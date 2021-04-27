namespace MZPO.AmoRepo
{
    public class ComplexResponse
    {
        public int Id { get; set; }
        public int? Contact_id { get; set; }
        public int? Company_id { get; set; }
        public string[] Request_id { get; set; }
        public bool Merged { get; set; }
    }
}