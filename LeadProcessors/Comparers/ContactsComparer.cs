using MZPO.AmoRepo;
using System;
using System.Collections.Generic;


namespace MZPO.LeadProcessors
{
    class ContactsComparer : IEqualityComparer<Contact>
    {
        public bool Equals(Contact x, Contact y)
        {
            if (Object.ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.id == y.id;
        }

        public int GetHashCode(Contact c)
        {
            if (c is null)
                return 0;

            int hashProductCode = c.id.GetHashCode();

            return hashProductCode;
        }
    }
}