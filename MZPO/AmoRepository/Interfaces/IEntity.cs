namespace MZPO.AmoRepo
{
    /// <summary>
    /// Interface of an entity in amoCRM.
    /// </summary>
    public interface IEntity
    {
#pragma warning disable IDE1006 // Naming Styles
        public static string entityLink { get; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
