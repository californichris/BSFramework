using System;
using System.Collections.Generic;
using System.Text;
using BS.Common.Dao.Handlers;
using BS.Common.Entities;
using BS.Common.Utils;

namespace BS.Common.Dao.Oracle
{
    /// <summary>
    /// This class implements the catalog related operations.
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public class CatalogOracleDAO : BaseSqlDAO, ICatalogDAO
    {
        /// <summary>
        /// Creates a CatalogOracleDAO instance with the default connection string.
        /// </summary>
        public CatalogOracleDAO() : this("")
        {
        }

        /// <summary>
        /// Creates a CatalogOracleDAO instance with the specified connection string.
        /// </summary>
        /// <param name="connString">The connection string</param>
        public CatalogOracleDAO(string connString) : base(connString, null, new OracleQueryBuilder())
        {
        }

        /// <summary>
        /// Return all entities related to the specified entity type.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <returns>The list of entities</returns>
        public virtual IList<Entity> GetEntities(Entity entity)
        {
            LoggerHelper.Info("Start");

            IList<Entity> list = new List<Entity>();
            try
            {
                StringBuilder query = GetQueryBuilder().BuildQuery(entity);
                LoggerHelper.Debug(query.ToString());

                ResultSetHandler<IList<Entity>> h = new EntityHandler<Entity>(entity);
                list = GetQueryRunner().Query(GetConnection(), query, h);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
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
        /// specified filter from an Oracle database.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="filter">The Filter</param>
        /// <returns>The filtered list of entities</returns>
        public virtual IList<Entity> GetEntities(Entity entity, FilterInfo filter)
        {
            LoggerHelper.Info("Start");

            IList<Entity> list = new List<Entity>();
            try
            {
                filter.Total = GetTotalRecords(entity);                              
                filter.FilteredRecords = GetFilteredTotalRecords(entity, filter);

                StringBuilder query = GetQueryBuilder().BuildQuery(entity, filter);
                LoggerHelper.Debug(query.ToString());

                ResultSetHandler<IList<Entity>> h = new EntityHandler<Entity>(entity);
                list = GetQueryRunner().Query(GetConnection(), query, h);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to fetch " + entity.GetTableName() + " list.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return list;
        }

        /// <summary>
        /// Find all entities that meet the specified entity type and properties 
        /// and applying the specified filter and search type from an Oracle database.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="filter">The filter</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The filtered list of entities</returns>
        public virtual IList<Entity> FindEntities(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();
            try
            {
                StringBuilder query = GetQueryBuilder().BuildFindEntitiesQuery(entity, filter, searchType);
                LoggerHelper.Debug(query.ToString());

                ResultSetHandler<IList<Entity>> h = new EntityHandler<Entity>(entity);
                list = GetQueryRunner().Query(GetConnection(), query, h);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to fetch " + entity.GetTableName() + " list.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return list;
        }

        /// <summary>
        /// Find all entities that meet the specified entity type and properties 
        /// and applying the specified search type from an SQL database.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The filtered list of entities</returns>
        public virtual IList<Entity> FindEntities(Entity entity, FilterInfo.SearchType searchType)
        {
            return FindEntities(entity, null, searchType);
        }

        /// <summary>        
        /// Find all entities that meet the specified entity type and properties from an Oracle database.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <returns>The filtered list of entities</returns>
        public virtual IList<Entity> FindEntities(Entity entity)
        {
            return FindEntities(entity, FilterInfo.SearchType.OR);
        }

        /// <summary>
        /// Saves the specified entity in a Oracle database.
        /// </summary>
        /// <param name="entity">The entity to be saved</param>
        public virtual void SaveEntity(Entity entity)
        {
            LoggerHelper.Info("Start");
            try
            {
                StringBuilder query;
                if (string.IsNullOrEmpty(entity.GetEntityId())) // New entity Insert
                {
                    query = GetQueryBuilder().BuildInsertQuery(entity);
                    LoggerHelper.Debug(query.ToString());
                    Int32 newId = GetQueryRunner().ExecuteScalar(GetConnection(), query);
                    entity.SetProperty(entity.GetFieldId().Name, newId.ToString());
                }
                else //update
                {
                    if (entity.GetFieldId() == null)
                    {
                        LoggerHelper.Error("Entity does not have an Id field defined.");
                    }

                    query = GetQueryBuilder().BuildUpdateQuery(entity);
                    LoggerHelper.Debug(query.ToString());
                    GetQueryRunner().ExecuteNonQuery(GetConnection(), query);
                }                         
            }
            catch (Exception e)
            {
                if (e.Message.Contains("UNIQUE KEY"))
                {
                    throw new Exception(entity.GetTableName() + " already exists.");
                }
                else
                {
                    throw new Exception("Unable to save " + entity.GetTableName() + ".", e);
                }
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Deletes the specified entity from an Oracle database.
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        public virtual void DeleteEntity(Entity entity)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (entity.GetFieldId() == null)
                {
                    LoggerHelper.Error("Entity does not have an Id field defined.");
                    return;
                }

                StringBuilder query = GetQueryBuilder().BuildDeleteQuery(entity);
                LoggerHelper.Debug(query.ToString());

                GetQueryRunner().ExecuteNonQuery(GetConnection(), query);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("FK") || e.Message.Contains("REFERENCE"))
                {
                    throw new Exception(entity.GetTableName() + " cannot be deleted, because is being used.");
                }
                else
                {
                    throw new Exception("Unable to delete " + entity.GetTableName() + " record.", e);
                }
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Updates the specified entity and then deletes the entity from the Oracle database as a single transacion.
        /// The purpose of this method is to be used when you have a delete trigger in the Oracle database and you need to update
        /// first who is doing the delete.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void UpdateDeleteEntity(Entity entity)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (entity.GetFieldId() == null)
                {
                    LoggerHelper.Error("Entity does not have an Id field defined.");
                    return;
                }

                IList<string> queries = new List<string>();
                StringBuilder query = GetQueryBuilder().BuildUpdateEntityQuery(entity);
                LoggerHelper.Debug(query.ToString());

                queries.Add(query.ToString());

                query = GetQueryBuilder().BuildDeleteQuery(entity);
                LoggerHelper.Debug(query.ToString());
                queries.Add(query.ToString());

                GetQueryRunner().ExecuteTransaction(GetConnection(), queries);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to update " + entity.GetTableName() + ".", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Updates the specified entity in a Oracle database.
        /// </summary>
        /// <param name="entity">The entity to be updated</param>
        public virtual void UpdateEntity(Entity entity)
        {
            LoggerHelper.Info("Start");
            try
            {
                StringBuilder query = GetQueryBuilder().BuildUpdateEntityQuery(entity);
                LoggerHelper.Debug(query.ToString());
                GetQueryRunner().ExecuteNonQuery(GetConnection(), query);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to update " + entity.GetTableName() + ".", e);               
        }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Updates all entities that math the specified whereEntity properties in an Oracle database.
        /// </summary>
        /// <param name="entity">The entity that contains the properties that will be updated.</param>
        /// <param name="whereEntity">The entity that contains the properties used in the WHERE clause.</param>
        public void UpdateEntity(Entity entity, Entity whereEntity)
        {
            throw new NotImplementedException("This method is not implemented.");
        }

        /// <summary>
        /// Deletes all entities that match the specified entity properties from an Oracle database.
        /// </summary>
        /// <param name="entity">The entity that contains the properties.</param>
        public virtual void DeleteEntities(Entity entity)
        {
            LoggerHelper.Info("Start");
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("DELETE FROM ").Append("[").Append(entity.GetTableName()).Append("]");

                StringBuilder where = new StringBuilder();

                foreach (KeyValuePair<string, string> pair in entity.GetProperties())
                {
                    if (!string.IsNullOrEmpty(pair.Value))
                    {
                        where.Append("[").Append(pair.Key).Append("]").Append(" = '").Append(pair.Value).Append("' AND ");
                    }
                }

                if (where.Length > 0)
                {
                    where.Remove(where.Length - 4, 4);
                    query.Append(" WHERE ").Append(where);
                }

                LoggerHelper.Debug(query.ToString());
                GetQueryRunner().ExecuteNonQuery(GetConnection(), query);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("FK") || e.Message.Contains("REFERENCE"))
                {
                    throw new Exception(entity.GetTableName() + " cannot be deleted, because is being used.");
                }
                else
                {
                    throw new Exception("Unable to delete " + entity.GetTableName() + " record.", e);
                }
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Executes a transaction in an Oracle database.
        /// </summary>
        /// <param name="operations">The list of operations to be executed.</param>
        public virtual void ExecuteTransaction(List<TransOperation> operations)
        {
            throw new NotImplementedException("This method is not available in offline mode.");
        }

        /// <summary>
        /// Returns the result of the specified aggregated function(s) executed in an Oracle Database.
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
        /// Returns the result of the specified aggregated function(s) executed in an Oracle Database.
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
    }
}