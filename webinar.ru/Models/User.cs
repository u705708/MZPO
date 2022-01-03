using System.Collections.Generic;

namespace MZPO.webinar.ru
{
    public class User
    {
#pragma warning disable IDE1006 // Naming Styles
        public long? id { get; set; }
        public long? userId { get; set; }
        public string name { get; set; }
        public string secondName { get; set; }
        public string company { get; set; }
        public string position { get; set; }
        public string organization { get; set; }
        public string phoneMain { get; set; }
        public string email { get; set; }
        public string nickName { get; set; }
        public string sex { get; set; }
        public List<AdditionalField> additionalFields { get; set; }
#pragma warning restore IDE1006 // Naming Styles    
    }
}