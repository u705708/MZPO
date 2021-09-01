using MZPO.ucheba.ru.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MZPO.ucheba.ru
{
    internal static class CredentialsProvider
    {
        internal static Credentials GetCredentials()
        {
            try
            {
                using FileStream stream = new("ucheba_cred.json", FileMode.Open, FileAccess.Read);
                using StreamReader sr = new(stream);
                return JsonConvert.DeserializeObject<Credentials>(sr.ReadToEndAsync().Result);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to process ucheba_cred.json: {e.Message}");
            }
        }
    }
}