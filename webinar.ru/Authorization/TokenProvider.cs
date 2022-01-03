using System.Threading;
using System.Threading.Tasks;

namespace MZPO.webinar.ru
{
    internal class TokenProvider
    {
        private readonly Credentials _credentials;
        private readonly SemaphoreSlim _webinarConnectionsSemaphore;

        internal TokenProvider()
        {
            _credentials = CredentialsProvider.GetCredentials().Result;
            _webinarConnectionsSemaphore = new(2, 2);
        }

        internal async Task<string> GetToken()
        {
            await _webinarConnectionsSemaphore.WaitAsync();
            return _credentials.xAuthToken;
        }

        internal void ReleaseToken()
        {
            _webinarConnectionsSemaphore.Release();
        }
    }
}