using MZPO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Services
{
    public class AmoAccount
    {
        public int id { get; set; }
        public string name { get; set; }
        public string subdomain { get; set; }
        public AuthProvider auth { get; set; }
        public DataProvider dataProvider { get; set; }
    }
}
