using System.Collections.Generic;
using BS.Common.Entities;
using BS.Common.Utils;

namespace BS.Common.Dao
{
    /// <summary>
    /// This interface defines the catalog related operations.
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public interface ICatalogDAO : IBaseDAO
    {
        /// <summary>
        /// Returns the list of all entities related with the specified entity type from a datasource.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <returns>The list of entities</returns>
        IList<Entity> GetEntities(Entity entity);
        
        /// <summary>
        /// Return the filtered list of entities related to the specified entity type and applying the
        /// specified filter from a datasource.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="filter">The Filter</param>
        /// <returns>The filtered list of entities</returns>
        IList<Entity> GetEntities(Entity entity, FilterInfo filter);
        
        /// <summary>
        /// Find all entities that meet the specified entity type and properties from a datasource.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <returns>The filtered list of entities</returns>
        IList<Entity> FindEntities(Entity entity);
        
        /// <summary>
        /// Find all entities that meet the specified entity type and properties 
        /// and applying the specified search type from a datasource.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The filtered list of entities</returns>
        IList<Entity> FindEntities(Entity entity, FilterInfo.SearchType searchType);
        
        /// <summary>
        /// Find all entities that meet the specified entity type and properties 
        /// and applying the specified filter and search type from a datasource.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="filter">The filter</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The filtered list of entities</returns>
        IList<Entity> FindEntities(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType);
        
        /// <summary>
        /// Saves the specified entity in the datasource.
        /// </summary>
        /// <param name="entity">The entity to be saved</param>
        void SaveEntity(Entity entity);
        
        /// <summary>
        /// Deletes the specified entity from the datasource.
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        void DeleteEntity(Entity entity);

        /// <summary>
        /// Updates the specified entity and the deletes the entity from the datasource as a single transacion.
        /// The purpose of this method is to be used when you have a delete trigger in the datasource and you need to update
        /// first who is doing the delete.
        /// </summary>
        /// <param name="entity"></param>
        void UpdateDeleteEntity(Entity entity);

        /// <summary>
        /// Updates the specified entity in the datasource.
        /// </summary>
        /// <param name="entity">The entity to be updated</param>
        void UpdateEntity(Entity entity);
        
        /// <summary>
        /// Deletes all entities that match the specified entity properties from the datasource.
        /// </summary>
        /// <param name="entity">The entity that contains the properties.</param>
        void DeleteEntities(Entity entity);

        void ExecuteTransaction(List<TransOperation> operations);

        /// <summary>
        /// Returns the total of records that match the specified entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>Total number of records</returns>
        int GetTotalRecords(Entity entity);
        
        /// <summary>
        /// Returns the number of records that match the specified entity properties and the specified filter info.
        /// </summary>
        /// <param name="entity">The entity that contain the properties</param>
        /// <param name="filter">The filter</param>
        /// <returns>The filtered number of records</returns>
        int GetFilteredTotalRecords(Entity entity, FilterInfo filter);

        IList<Entity> GetAggregateEntities(Entity entity, AggregateInfo aggregateInfo, FilterInfo.SearchType searchType);

        IList<Entity> GetAggregateEntities(Entity entity, AggregateInfo aggregateInfo, FilterInfo.SearchType searchType, FilterInfo filter);
    }
}