namespace MZPO.AmoRepo
{
    /// <summary>
    /// EntityLink is a request to link to entites together.
    /// </summary>
    public class EntityLink
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Id of the linked entity.
        /// </summary>
        public int to_entity_id { get; set; }
        /// <summary>
        /// Type of the linked entity.
        /// </summary>
        public string to_entity_type { get; set; }
        /// <summary>
        /// Id of the entity.
        /// </summary>
        public int? entity_id { get; set; }
        /// <summary>
        /// Type of the entity.
        /// </summary>
        public string entity_type { get; set; }

        public MetaData metadata { get; set; }

        public class MetaData
        {
            public int quantity { get; set; }
            public int catalog_id { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}