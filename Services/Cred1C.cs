using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

            try
            {
                using StreamReader sr = new("1c_cred.json");
                JsonConvert.PopulateObject(sr.ReadToEndAsync().Result, _credentials1C);
            }
            catch
            {
                new Log().Add("Unable to read 1c_cred.json");
            }
        }

        public Credentials1C GetCredentials()
        {
            return _credentials1C;
        }
    }
}