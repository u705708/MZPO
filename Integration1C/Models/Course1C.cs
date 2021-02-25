using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Course1C
    {
#pragma warning disable IDE1006 // Naming Styles
        public int product_id_1C { get; set; }
        public List<Amo_id> amo_ids { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public int price { get; set; }
        public int duration { get; set; }
        public string format { get; set; }
        public string program_id { get; set; }
        public string group { get; set; }
        public string requirements { get; set; }
        public string supplementary_info { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
