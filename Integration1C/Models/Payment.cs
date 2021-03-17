using System;

namespace Integration1C
{
    public class Payment
    {
#pragma warning disable IDE1006 // Naming Styles
        public DateTime payment_date { get; set; }
        public int payment_amount { get; set; }
        public Guid client_id_1C { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
