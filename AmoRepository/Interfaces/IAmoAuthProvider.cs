using System.Threading;
using System.Threading.Tasks;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Interface of an amoCRM authentication provider.
    /// </summary>
    public interface IAmoAuthProvider
    {
        /// <summary>
        /// Returns amoCRM authentication token.
        /// </summary>
        public Task<string> GetToken();

        /// <summary>
        /// Returns amoCRM account id.
        /// </summary>
        public int GetAccountId();

        /// <summary>
        /// Returns semaphore to ratelimit outgoing requests.
        /// </summary>
        public SemaphoreSlim GetSemaphoreSlim();

        /// <summary>
        /// Refreshes auth credential from db.
        /// </summary>
        public Task RefreshAmoAccountFromDBAsync();
    }
}