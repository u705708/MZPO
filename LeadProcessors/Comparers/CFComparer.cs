using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MZPO.AmoRepo;
using MZPO.Services;

namespace MZPO.LeadProcessors
{
    class CFComparer : IEqualityComparer<Custom_fields_value>
    {
        public bool Equals(Custom_fields_value x, Custom_fields_value y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return x.field_id == y.field_id;
        }

        public int GetHashCode(Custom_fields_value cf)
        {
            if (cf is null) return 0;

            int hashProductCode = cf.field_id.GetHashCode();

            return hashProductCode;
        }
    }
}
