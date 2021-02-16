using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MZPO.AmoRepo;
using MZPO.Services;

namespace MZPO.LeadProcessors
{
    class TagComparer : IEqualityComparer<Tag>
    {
        public bool Equals(Tag x, Tag y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return x.name == y.name;
        }

        public int GetHashCode(Tag tag)
        {
            if (tag is null) return 0;

            int hashProductName = tag.name == null ? 0 : tag.name.GetHashCode();

            return hashProductName;
        }
    }
}
