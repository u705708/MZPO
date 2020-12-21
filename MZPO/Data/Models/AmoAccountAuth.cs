using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Data
{
    public class AmoAccountAuth
    {
        public int id { get; set; }
        public string name { get; set; }
        public string subdomain { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string redirect_uri { get; set; }
        public string code { get; set; }
        public string authToken { get; set; }
        public string refrToken { get; set; }
        public DateTime validity { get; set; }
    }
}
