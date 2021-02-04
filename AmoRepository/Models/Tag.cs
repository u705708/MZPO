namespace MZPO.AmoRepo
{
    /// <summary>
    /// Tag in amoCRM applied to entities such as Lead, Contact.
    /// </summary>
    public class Tag
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Tag id.
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Tag name.
        /// </summary>
        public string name { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
