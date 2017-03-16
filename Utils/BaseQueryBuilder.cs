using System;
using BS.Common.Entities;

namespace BS.Common.Utils
{
    /// <summary>
    /// Common methods for all QueryBuilder implementations
    /// </summary>
    /// <history>
    ///     <change date="12/01/2016" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public abstract class BaseQueryBuilder
    {
        /// <summary>
        /// Validates if the specified entity is not null and the table name is valid.
        /// </summary>
        /// <param name="entity">The entity to be checked</param>
        protected virtual void CheckNulls(Entity entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }
        }

        /// <summary>
        /// Validates if the specified instance is not null.
        /// </summary>
        /// <param name="instance">The instance to be checked</param>
        protected virtual void CheckNulls(Object instance)
        {
            if (instance == null)
            {
                LoggerHelper.Warning("An instance must be specified.");
                throw new ArgumentNullException("An instance must be specified.");
            }
        }
    }
}
