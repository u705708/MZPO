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
        public string GetToken();

        /// <summary>
        /// Returns amoCRM account id.
        /// </summary>
        public int GetAccountId();
    }
}
