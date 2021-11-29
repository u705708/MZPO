using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class Course1C
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid? product_id_1C { get; set; }
        private List<Amo_id> _amo_ids { get; set; }
        public List<Amo_id> amo_ids
        {
            get
            {
                return _amo_ids is null ? null : _amo_ids.Where(x => x.account_id != 29490250).ToList();
            }
            set
            {
                _amo_ids = value;
            }
        }
        public string name { get; set; }
        public string short_name { get; set; }
        public List<PriceItem> ItemPrices { get; set; }
        public int duration { get; set; }
        public string format { get; set; }
        //public string program_id { get; set; }
        //public string group { get; set; }
        //public string requirements { get; set; }
        public string supplementary_info { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}