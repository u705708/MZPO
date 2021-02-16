using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Client
    {
#pragma warning disable IDE1006 // Naming Styles
        public int client_id_1C { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        public string sex { get; set; }
        public string dob { get; set; }
        public string pass_serie { get; set; }
        public string pass_number { get; set; }
        public string pass_issued_by { get; set; }
        public string pass_issued_at { get; set; }
        public string pass_dpt_code { get; set; }
        public string address { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}