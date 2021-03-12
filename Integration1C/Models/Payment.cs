using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Payment
    {
        public DateTime Payment_date { get; set; }
        public int Payment_amount { get; set; }
        public Guid Client_id_1C { get; set; }
    }
}
