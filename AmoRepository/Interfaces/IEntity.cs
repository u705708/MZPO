using System.Collections.Generic;

namespace MZPO.AmoRepo
{
    /// <summary>
    /// Interface of an entity in amoCRM.
    /// </summary>
    public interface IEntity
    {
#pragma warning disable IDE1006 // Naming Styles
        public static string entityLink { get; }
        public List<Custom_fields_value> custom_fields_values { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
