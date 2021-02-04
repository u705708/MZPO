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
    }
}
