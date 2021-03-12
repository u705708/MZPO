using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Company1C
    {
        public Guid Company_id_1C { get; set; }
        public List<Amo_id> Amo_ids { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Signee { get; set; }
        public string OGRN { get; set; }
        public string INN { get; set; }
        public string Acc_no { get; set; }
        public string KPP { get; set; }
        public string BIK { get; set; }
        public string Address { get; set; }
        public string LPR_name { get; set; }
        public string Post_address { get; set; }
    }
}