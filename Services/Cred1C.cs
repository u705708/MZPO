using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZPO.Services
{
    public class Cred1C
    {
        private readonly Credentials1C _credentials1C;

        public Cred1C()
        {
            _credentials1C = new();
        }

        public Credentials1C GetCredentials()
        {
            return _credentials1C;
        }
    }
}
