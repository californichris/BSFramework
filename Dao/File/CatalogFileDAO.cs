using System;
using System.Collections.Generic;
using System.Linq;
using BS.Common.Entities;
using BS.Common.Utils;

namespace BS.Common.Dao.File
{
    /// <summary>
    /// ICatalogDAO implementation that returns the data from JSON-formatted text files.
    /// </summary>
    public class CatalogFileDAO : BaseFileDAO, ICatalogDAO
    {
        /// <summary>
        /// Creates a CatalogFileDAO instance.
        /// </summary>
        public CatalogFileDAO():base()
        {
        }

        /// <summary>
        /// Returns the list of all entities related with the specified entity type from a JSON-formatted text file.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <returns>The list of entities</returns>
        public IList<Entity> GetEntities(Entity entity){
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();

            try
            {           
                string json = LoadFile(entity);
                list = DeserializeEntityList(entity, json);
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Unable to fetch " + entity.GetTableName() + " list.", e);
                throw new Exception("Unable to fetch " + entity.GetTableName() + " list.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return list;
        }

        /// <summary>
        /// Return the filtered list of entities related to the specified entity type and applying the
        /// specified filter from a JSON-formatted text file.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="filter">The Filter</param>
        /// <returns>The filtered list of entities</returns>
        public IList<Entity> GetEntities(Entity entity, FilterInfo filter)
        {
            LoggerHelper.Info("Start");
            List<Entity> filteredItems = new List<Entity>();
            try
            {
                IList<Entity> list = GetEntities(entity);
                filter.Total = list.Count;

                filteredItems = (List<Entity>)list;

                //Merging tables
                MergeData(filteredItems, entity);

                //Filtering entities
                filteredItems = FilterEntities(list, entity, filter);

                //Sorting results
                if (filter != null && filter.SortColumns.Count > 0)
                {
                    ((List<Entity>)filteredItems).Sort((x, y) => SortEntities(x, y, filter));
                }

                //Returning only the number of records requested
                filter.FilteredRecords = filteredItems.Count;
                if (filter.Lenght > 0)
                {
                    int length = filteredItems.Count < filter.Lenght ? filteredItems.Count : filter.Lenght;
                    Entity[] array = new Entity[length];
                    filteredItems.CopyTo(filter.Start, array, 0, length);

                    filteredItems = array.ToList();
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Unable to fetch " + entity.GetTableName() + " list.", e);
                throw new Exception("Unable to fetch " + entity.GetTableName() + " list.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return filteredItems;
        }

        /// <summary>
        /// Find all entities that meet the specified entity type and properties 
        /// and applying the specified filter and search type from a JSON-formatted text file.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="filter">The filter</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The filtered list of entities</returns>
        public IList<Entity> FindEntities(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType)
        {
            LoggerHelper.Info("Start");

            List<Entity> filteredItems = new List<Entity>();
            try
            {
                string json = LoadFile(entity);
                
                //TODO: add logic to get pageconfig and create entity fields when fields.count is 0
                List<Entity> list = (List<Entity>) DeserializeEntityList(entity, json);

                //Filtering
                List<Entity> filteredAND = list;

                //Merging tables
                MergeData(filteredAND, entity);
                
                if (searchType == FilterInfo.SearchType.AND)
                {
                    foreach (KeyValuePair<string, string> pair in entity.GetProperties())
                    {
                        if (!string.IsNullOrEmpty(pair.Value))
                        {
                            filteredAND = filteredAND.FindAll(ent => ent.GetProperty(pair.Key).Equals(pair.Value));
                        }
                    }

                    filteredItems = filteredAND;
                }
                else
                {
                    filteredItems = list.FindAll(ent => OrFilter(ent, entity));
                }

                //Sorting results
                if (filter != null && filter.SortColumns.Count > 0)
                {
                    ((List<Entity>)list).Sort((x, y) => SortEntities(x, y, filter));
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Unable to fetch " + entity.GetTableName() + " list.", e);
                throw new Exception("Unable to fetch " + entity.GetTableName() + " list.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return filteredItems;
        }

        /// <summary>
        /// Find all entities that meet the specified entity type and properties 
        /// and applying the specified search type from a JSON-formatted text file.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The filtered list of entities</returns>
        public IList<Entity> FindEntities(Entity entity, FilterInfo.SearchType searchType)
        {
            return FindEntities(entity, null, searchType);
        }

        /// <summary>
        /// Find all entities that meet the specified entity type and properties from a JSON-formatted text file.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <returns>The filtered list of entities</returns>
        public IList<Entity> FindEntities(Entity entity)
        {
            return FindEntities(entity, FilterInfo.SearchType.OR);
        }

        /// <summary>
        /// Saves the specified entity in a file.
        /// </summary>
        /// <param name="entity">The entity to be saved</param>
        public void SaveEntity(Entity entity)
        {
            LoggerHelper.Info("Start");
            if (string.IsNullOrEmpty(entity.GetEntityId())) // New entity Insert
            {
                throw new NotImplementedException("This method is not available in offline mode.");   
            }
            
            //Update existing entity
            try
            {
                IList<Entity> list = GetEntities(entity);

                int index = ((List<Entity>) list).FindIndex(ent => ent.GetEntityId().Equals(entity.GetEntityId()));
                if (index == -1)
                {
                    LoggerHelper.Error("Existing entity could not be found.");
                }

                list[index] = entity;
                SaveToFile(list, GetFileName(entity));
            }
            catch (Exception e)
            {
                throw new Exception("Unable to save " + entity.GetTableName() + ".", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }            
        }

        /// <summary>
        /// This method is not implemented for file type catalogs.
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        public void DeleteEntity(Entity entity)
        {
            throw new NotImplementedException("This method is not available in offline mode.");   
        }

        /// <summary>
        /// Updates the specified entity and the deletes the entity from the datasource as a single transacion.
        /// The purpose of this method is to be used when you have a delete trigger in the datasource and you need to update
        /// first who is doing the delete.
        /// </summary>
        /// <param name="entity"></param>
        public void UpdateDeleteEntity(Entity entity)
        {
            throw new NotImplementedException("This method is not available in offline mode."); 
        }

        /// <summary>
        /// This method is not implemented for file type catalogs.
        /// </summary>
        /// <param name="entity">The entity to be updated</param>
        public void UpdateEntity(Entity entity)
        {
            throw new NotImplementedException("This method is not available in offline mode.");       
        }

        /// <summary>
        /// Updates all entities that math the specified whereEntity properties in the file datasource.
        /// </summary>
        /// <param name="entity">The entity that contains the properties that will be updated.</param>
        /// <param name="whereEntity">The entity that contains the properties used in the WHERE clause.</param>
        public void UpdateEntity(Entity entity, Entity whereEntity)
        {
            throw new NotImplementedException("This method is not available in offline mode.");  
        }

        /// <summary>
        /// This method is not implemented for file type catalogs.
        /// </summary>
        /// <param name="entity">The entity that contains the properties.</param>
        public void DeleteEntities(Entity entity)
        {
            throw new NotImplementedException("This method is not available in offline mode.");               
        }

        /// <summary>
        /// Executes a transaction in the file datasource.
        /// </summary>
        /// <param name="operations">The list of operations to be executed.</param>
        public virtual void ExecuteTransaction(List<TransOperation> operations)
        {
            throw new NotImplementedException("This method is not available in offline mode.");               
        }

        /// <summary>
        /// Returns the result of the specified aggregated function(s).
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="aggregateInfo">The aggregateInfo data</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The agregated list of entities</returns>
        public virtual IList<Entity> GetAggregateEntities(Entity entity, AggregateInfo aggregateInfo, FilterInfo.SearchType searchType)
        {
            throw new NotImplementedException("This method is not available in offline mode."); 
        }

        /// <summary>
        /// Returns the result of the specified aggregated function(s).
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="aggregateInfo">The aggregateInfo data</param>
        /// <param name="searchType">The search type</param>
        /// <param name="filter">The filter info</param>
        /// <returns>The agregated list of entities</returns>
        public IList<Entity> GetAggregateEntities(Entity entity, AggregateInfo aggregateInfo, FilterInfo.SearchType searchType, FilterInfo filter)
        {
            throw new NotImplementedException("This method is not available in offline mode."); 
        }

        /// <summary>
        /// This mehod is not implemented for files, always returns 0.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>Always returns 0</returns>
        public int GetTotalRecords(Entity entity)
        {
            return 0;
        }

        /// <summary>
        /// This mehod is not implemented for files, always returns 0.
        /// </summary>
        /// <param name="entity">The entity that contain the properties</param>
        /// <param name="filter">The filter</param>
        /// <returns>Always returns 0</returns>
        public int GetFilteredTotalRecords(Entity entity, FilterInfo filter)
        {
            return 0;
        }

        private bool OrFilter(Entity ent, Entity entity)
        {
            foreach (KeyValuePair<string, string> pair in entity.GetProperties())
            {
                if (!string.IsNullOrEmpty(pair.Value) && ent.GetProperty(pair.Key).Equals(pair.Value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}