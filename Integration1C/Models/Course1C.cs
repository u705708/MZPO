using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Course1C
    {
        public Guid Product_id_1C { get; set; }
        public List<Amo_id> Amo_ids { get; set; }
        public string Name { get; set; }
        public string Short_name { get; set; }
        public int Price { get; set; }
        public int Duration { get; set; }
        public string Format { get; set; }
        public string Program_id { get; set; }
        public string Group { get; set; }
        public string Requirements { get; set; }
        public string Supplementary_info { get; set; }
    }
}