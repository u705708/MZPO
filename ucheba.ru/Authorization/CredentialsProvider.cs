using MZPO.ucheba.ru.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MZPO.ucheba.ru
{
    internal static class CredentialsProvider
    {
        internal static async Task<Credentials> GetCredentials()
        {
            try
            {
                using FileStream stream = new("ucheba_cred.json", FileMode.Open, FileAccess.Read);
                using StreamReader sr = new(stream);
                return JsonConvert.DeserializeObject<Credentials>(await sr.ReadToEndAsync());
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Unable to process ucheba_cred.json: {e.Message}");
            }
        }
    }
}