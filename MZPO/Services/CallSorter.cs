using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Services
{
    public class CallSorter
    {
        private bool marker;

        public CallSorter()
        {
            marker = true;
        }

        public int GetChoice()
        {
            marker = !marker;
            return marker ? 1 : 0;
        }
    }
}
