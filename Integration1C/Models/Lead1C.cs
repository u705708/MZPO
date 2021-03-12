using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Lead1C
    {
        public Guid Lead_id_1C { get; set; }
        public List<Amo_id> Amo_ids { get; set; }
        public Guid Client_id_1C { get; set; }
        public Guid Product_id_1C { get; set; }
        public Guid? Company_id_1C { get; set; }
        public string Organization { get; set; }
        public int Price { get; set; }
        public bool Is_corporate { get; set; }
        public string Lead_status { get; set; }
        public string Marketing_channel { get; set; }
        public string Marketing_source { get; set; }
        public string Author { get; set; }
        public string Responsible_user { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
