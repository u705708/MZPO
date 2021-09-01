using MZPO.ucheba.ru.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MZPO.ucheba.ru
{
    internal static class TokenProvider
    {
        private static Token GetCurrentToken()
        {
            try
            {
                using FileStream stream = new("ucheba_token.json", FileMode.Open, FileAccess.Read);
                using StreamReader sr = new(stream);
                return JsonConvert.DeserializeObject<Token>(sr.ReadToEndAsync().Result);
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to read ucheba_token.json: {e.Message}");
            }
        }

        private static void SaveNewToken(Token token)
        {
            using StreamWriter sw = new("ucheba_token.json", false, System.Text.Encoding.Default);
            sw.WriteLine(JsonConvert.SerializeObject(token, Formatting.Indented));
        }

        private static Token UpdateToken()
        {
            return Auth.GetNewToken();
        }

        internal static string GetAuthToken()
        {
            Token token = GetCurrentToken();

            if (token.expiresAt <= DateTime.Now.AddSeconds(-5))
            {
                token = UpdateToken();
                SaveNewToken(token);
            }

            return token.token;
        }
    }
}