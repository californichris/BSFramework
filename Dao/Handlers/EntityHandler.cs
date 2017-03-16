using System.Collections.Generic;
using System.Data.Common;
using BS.Common.Entities;

namespace BS.Common.Dao.Handlers
{
    /// <summary>
    /// ResultSetHandler implementation that converts a
    /// ResultSet into a List of Entities.
    /// </summary>
    /// <typeparam name="T">The target Entity type</typeparam>
    public class EntityHandler<T> : ResultSetHandler<IList<T>>
    {
        /// <summary>
        /// The Type of Entity produced by this handler.
        /// </summary>
        private Entity entType;
        
        /// <summary>
        /// The RowProcessor implementation to use when converting rows into entities.
        /// </summary>
        private RowProcessor processor;

        /// <summary>
        /// Creates a new instance of EntityHandler.
        /// </summary>
        /// <param name="entity">The Entity Type that objects returned from handle() are created from.</param>
        public EntityHandler(Entity entity)
            : this(entity, new EntityRowProcessor(entity))
        {
        }

        /// <summary>
        /// Creates a new instance of EntityHandler.
        /// </summary>
        /// <param name="entity">The Entity Type that objects returned from handle() are created from.</param>
        /// <param name="processor">The RowProcessor implementation to use when converting rows into entities.</param>
        public EntityHandler(Entity entity, RowProcessor processor)
        {
            this.entType = entity;
            this.processor = processor;
        }

        /// <summary>
        /// Convert the whole ResultSet into a List of Entities with
        /// the Type given in the constructor.
        /// </summary>
        /// <param name="rs">The ResultSet to handle.</param>
        /// <returns>A List of entities</returns>
        public IList<T> Handle(DbDataReader rs) 
        {
            return processor.ToBeanList<T>(rs, this.entType.GetType());
        }
    }
}
