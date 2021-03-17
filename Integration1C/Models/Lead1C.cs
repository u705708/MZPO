using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Lead1C
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid? lead_id_1C { get; set; }
        public List<Amo_id> amo_ids { get; set; }
        public Guid client_id_1C { get; set; }
        public Guid product_id_1C { get; set; }
        public Guid? company_id_1C { get; set; }
        public string organization { get; set; }
        public int price { get; set; }
        public bool is_corporate { get; set; }
        public string lead_status { get; set; }
        public string marketing_channel { get; set; }
        public string marketing_source { get; set; }
        public string author { get; set; }
        public string responsible_user { get; set; }
        public List<Payment> payments { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}