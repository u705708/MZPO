using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Client1C
    {
        public Guid Client_id_1C { get; set; }
        public List<Amo_id> Amo_ids { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public DateTime Dob { get; set; }
        public string Pass_serie { get; set; }
        public string Pass_number { get; set; }
        public string Pass_issued_by { get; set; }
        public string Pass_issued_at { get; set; }
        public string Pass_dpt_code { get; set; }
    }
}