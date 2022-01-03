using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MZPO.webinar.ru
{
    internal static class CredentialsProvider
    {
        internal static async Task<Credentials> GetCredentials()
        {
            try
            {
                using FileStream stream = new("webinar_token.json", FileMode.Open, FileAccess.Read);
                using StreamReader sr = new(stream);
                return JsonConvert.DeserializeObject<Credentials>(await sr.ReadToEndAsync());
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Unable to process webinar_token.json: {e.Message}");
            }
        }
    }
}