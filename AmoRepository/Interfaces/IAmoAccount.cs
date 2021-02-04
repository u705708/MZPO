namespace MZPO.AmoRepo
{
    /// <summary>
    /// Interface of an amoCRM account.
    /// </summary>
    public interface IAmoAccount
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// amoCRM account id.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// amoCRM account name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// amoCRM account subdomain.
        /// </summary>
        public string subdomain { get; set; }

        /// <summary>
        /// amoCRM account authentication provider.
        /// </summary>
        public IAmoAuthProvider auth { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
