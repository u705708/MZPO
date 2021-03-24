using MZPO.AmoRepo;

namespace MZPO.Services
{
    public class AmoAccount : IAmoAccount
    {
#pragma warning disable IDE1006 // Naming Styles
        public int id { get; set; }
        public string name { get; set; }
        public string subdomain { get; set; }
        public IAmoAuthProvider auth { get; set; }
        public DataProvider dataProvider { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}