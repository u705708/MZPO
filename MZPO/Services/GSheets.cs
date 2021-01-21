using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.IO;

namespace MZPO.Services
{
    public class GSheets
    {
        public SheetsService GetService()
        {
            GoogleCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new string[] { SheetsService.Scope.Spreadsheets });
            }

            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "mzpo2amo",
            });
        }
    }
}