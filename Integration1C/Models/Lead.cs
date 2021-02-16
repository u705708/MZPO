using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Lead
    {
#pragma warning disable IDE1006 // Naming Styles
        public int lead_id_1C { get; set; }
        public int client_id_1C { get; set; }
        public int product_id_1C { get; set; }
        public int? company_id_1C { get; set; }
        public string organization { get; set; }
        public int price { get; set; }
        public bool is_corporate { get; set; }
        public string lead_status { get; set; }
        public string marketing_channel { get; set; }
        public string marketing_source { get; set; }
        public string author { get; set; }
        public string responsible_user { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
