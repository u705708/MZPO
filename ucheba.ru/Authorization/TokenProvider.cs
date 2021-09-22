using MZPO.ucheba.ru.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MZPO.ucheba.ru
{
    internal static class TokenProvider
    {
        internal static async Task<string> GetAuthToken()
        {
            Token token = await GetCurrentToken();

            if (token.expiresAt <= DateTime.Now.AddSeconds(-5))
            {
                token = await UpdateToken();
                await SaveNewToken(token);
            }

            return token.token;
        }

        private static async Task<Token> GetCurrentToken()
        {
            try
            {
                using FileStream stream = new("ucheba_token.json", FileMode.Open, FileAccess.Read);
                using StreamReader sr = new(stream);
                return JsonConvert.DeserializeObject<Token>(await sr.ReadToEndAsync());
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to read ucheba_token.json: {e.Message}");
            }
        }

        private static async Task SaveNewToken(Token token)
        {
            using StreamWriter sw = new("ucheba_token.json", false, System.Text.Encoding.Default);
            await sw.WriteLineAsync(JsonConvert.SerializeObject(token, Formatting.Indented));
        }

        private static async Task<Token> UpdateToken()
        {
            return await Auth.GetNewToken();
        }
    }
}