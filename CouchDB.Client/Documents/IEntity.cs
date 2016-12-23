namespace CouchDB.Client
{
    /// <summary>
    /// Represents Entity.
    /// All objects that need to be treated as entities should implement this interface.
    /// Entity will automatically reflect the database status after each operation, 
    /// so it can repeatedly be passed to all kinds of operations through <see cref="CouchDB.Client.CouchDBDatabase"/>.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// ID of entity.
        /// </summary>
        string _id { get; set; }

        /// <summary>
        /// Revision of entity.
        /// </summary>
        string _rev { get; set; }
    }
}
