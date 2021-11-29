using System;
using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public class Lead1C
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid? lead_id_1C { get; set; }
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
        public Guid? client_id_1C { get; set; }
        public Guid? product_id_1C { get; set; }
        public Guid? company_id_1C { get; set; }
        public string organization { get; set; }
        public int price { get; set; }
        public bool is_corporate { get; set; }
        public string lead_status { get; set; }
        public string author { get; set; }
        public string responsible_user { get; set; }
        public List<Payment> payments { get; set; }

        private string _marketing_channel;
        public string marketing_channel 
        { get
            { return _marketing_channel; }
            set
            { _marketing_channel = value.Length > 255? value.Substring(0, 255) : value; } 
        }

        private string _marketing_source;
        public string marketing_source
        {
            get
            {return _marketing_source; }
            set
            { _marketing_source = value.Length > 255 ? value.Substring(0, 255) : value; }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}