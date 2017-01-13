using System;
using System.Collections.Generic;
using System.Text;
using BS.Common.Entities;
using BS.Common.Utils;
using BS.Common.Dao.Handlers;
using System.Collections;
using System.Data;

namespace BS.Common.Dao.Sql
{
    /// <summary>
    /// ICatalogDAO implementation that returns the data from an SQL database
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public class CatalogSqlDAO : BaseSqlDAO, ICatalogDAO
    {

        /// <summary>
        /// Creates a CatalogSqlDAO instance with the default connection string.
        /// </summary>
        public CatalogSqlDAO():base()
        {
        }

        /// <summary>
        /// Creates a CatalogSqlDAO instance with the specified connection string.
        /// </summary>
        /// <param name="connString">The connection string</param>
        public CatalogSqlDAO(string connString)
            : base(connString)
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
        /// specified filter from an SQL database.
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

                StatementWrapper stmtWrapper = GetQueryBuilder().BuildSelectStatement(entity, filter);
                LoggerHelper.Debug(stmtWrapper.Query.ToString());                

                ResultSetHandler<IList<Entity>> h = new EntityHandler<Entity>(entity);
                list = GetQueryRunner().Query(GetConnection(), stmtWrapper, h);
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
        /// and applying the specified filter and search type from an SQL database.
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
                StatementWrapper stmtWrapper = GetQueryBuilder().BuildFindEntitiesStatement(entity, filter, searchType);
                LoggerHelper.Debug(stmtWrapper.Query.ToString());

                ResultSetHandler<IList<Entity>> h = new EntityHandler<Entity>(entity);
                list = GetQueryRunner().Query(GetConnection(), stmtWrapper, h);
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
        /// Find all entities that meet the specified entity type and properties from an SQL database.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <returns>The filtered list of entities</returns>
        public virtual IList<Entity> FindEntities(Entity entity)
        {
            return FindEntities(entity, FilterInfo.SearchType.OR);
        }

        /// <summary>
        /// Saves the specified entity in a SQL database.
        /// </summary>
        /// <param name="entity">The entity to be saved</param>
        public virtual void SaveEntity(Entity entity)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (string.IsNullOrEmpty(entity.GetEntityId())) // New entity Insert
                {
                    StatementWrapper stmtWrapper = GetQueryBuilder().BuildInsertStatement(entity);
                    LoggerHelper.Debug(stmtWrapper.Query.ToString());
                    Int32 newId = GetQueryRunner().ExecuteScalar(GetConnection(), stmtWrapper);
                    entity.SetProperty(entity.GetFieldId().Name, newId.ToString());
                }
                else //update
                {
                    StatementWrapper stmtWrapper = GetQueryBuilder().BuildUpdateStatement(entity);
                    LoggerHelper.Debug(stmtWrapper.Query.ToString());
                    GetQueryRunner().ExecuteNonQuery(GetConnection(), stmtWrapper);
                }                         
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
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
        /// Deletes the specified entity from an SQL database.
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

                StatementWrapper stmtWrapper = GetQueryBuilder().BuildDeleteStatement(entity);
                LoggerHelper.Debug(stmtWrapper.Query.ToString());

                GetQueryRunner().ExecuteNonQuery(GetConnection(), stmtWrapper);
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
        /// Updates the specified entity and then deletes the entity from the SQL database as a single transacion.
        /// The purpose of this method is to be used when you have a delete trigger in the SQL database and you need to update
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

                IList<StatementWrapper> statements = new List<StatementWrapper>();
                StatementWrapper stmtWrapper = GetQueryBuilder().BuildUpdateEntityStatement(entity);
                LoggerHelper.Debug(stmtWrapper.Query.ToString());

                statements.Add(stmtWrapper);

                stmtWrapper = GetQueryBuilder().BuildDeleteStatement(entity);
                LoggerHelper.Debug(stmtWrapper.Query.ToString());
                statements.Add(stmtWrapper);

                GetQueryRunner().ExecuteTransaction(GetConnection(), statements);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to delete " + entity.GetTableName() + ".", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Updates the specified entity in a SQL database.
        /// </summary>
        /// <param name="entity">The entity to be updated</param>
        public virtual void UpdateEntity(Entity entity)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (entity.GetFieldId() == null)
                {
                    LoggerHelper.Error("Entity does not have an Id field defined.");
                    return;
                }

                StatementWrapper stmtWrapper = GetQueryBuilder().BuildUpdateEntityStatement(entity);
                LoggerHelper.Debug(stmtWrapper.Query.ToString());
                GetQueryRunner().ExecuteNonQuery(GetConnection(), stmtWrapper);
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
        /// Updates all entities that math the specified whereEntity properties in a SQL database.
        /// </summary>
        /// <param name="entity">The entity that contains the properties that will be updated.</param>
        /// <param name="whereEntity">The entity that contains the properties used in the WHERE clause.</param>
        public virtual void UpdateEntity(Entity entity, Entity whereEntity)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (whereEntity == null)
                {
                    UpdateEntity(entity);
                    return;
                }

                StatementWrapper stmtWrapper = ((QueryBuilder) GetQueryBuilder()).BuildUpdateEntityStatement(entity, whereEntity);
                LoggerHelper.Debug(stmtWrapper.Query.ToString());
                GetQueryRunner().ExecuteNonQuery(GetConnection(), stmtWrapper);
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
        /// Deletes all entities that match the specified entity properties from a SQL database.
        /// </summary>
        /// <param name="entity">The entity that contains the properties.</param>
        public virtual void DeleteEntities(Entity entity)
        {
            LoggerHelper.Info("Start");
            try
            {
                GetQueryRunner().ExecuteNonQuery(GetConnection(), GetQueryBuilder().BuildDeleteEntitiesStatement(entity));
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
        /// Executes a transaction in a SQL database.
        /// </summary>
        /// <param name="operations">The list of operations to be executed.</param>
        public virtual void ExecuteTransaction(List<TransOperation> operations)
        {
            LoggerHelper.Info("Start");
            try
            {
                StringBuilder query = new StringBuilder();
                IList<DBParam> queryParams = new List<DBParam>();
                
                StringBuilder variables = new StringBuilder();
                StringBuilder statementsQueries = new StringBuilder();
                
                IDictionary<string, string> defaultVals = new Dictionary<string, string>();                
                
                IQueryBuilder queryBuilder = GetQueryBuilder();
                
                foreach (TransOperation operation in operations)
                {
                    Entity entity = operation.Entity;
                    if (operation.OperationType == TransOperation.OperType.Save)
                    {
                        if (string.IsNullOrEmpty(operation.Entity.GetEntityId())) // New entity Insert
                        {
                            if (operation.BindIds)
                            {
                                queryBuilder.BuildInsertStatement(entity, statementsQueries, queryParams, defaultVals);
                            }
                            else
                            {
                                queryBuilder.BuildInsertStatement(entity, statementsQueries, queryParams);
                            }
                            
                            Field id = entity.GetFieldId();
                            string varName = GetVariableName(id);

                            if (!defaultVals.ContainsKey(id.Name))
                            {
                                defaultVals[id.Name] = varName;
                                variables.Append("DECLARE ").Append(varName).Append(" AS INT\n");
                            }

                            statementsQueries.Append("\nSET ").Append(varName).Append(" = scope_identity()\n");
                        }
                        else //update
                        {
                            queryBuilder.BuildUpdateStatement(entity, statementsQueries, queryParams);
                        }
                    }
                    else if (operation.OperationType == TransOperation.OperType.Update)
                    {
                        queryBuilder.BuildUpdateEntityStatement(entity, statementsQueries, queryParams);
                    }
                    else if (operation.OperationType == TransOperation.OperType.Delete)
                    {
                        queryBuilder.BuildDeleteStatement(entity, statementsQueries, queryParams);
                    }
                    else if (operation.OperationType == TransOperation.OperType.DeleteEntities)
                    {
                        queryBuilder.BuildDeleteEntitiesStatement(entity, statementsQueries, queryParams);
                    }

                    statementsQueries.Append("\n");
                }

                query.Append(variables).Append("\n");
                query.Append(statementsQueries);
                LoggerHelper.Debug(query.ToString());

                StatementWrapper stmtWrapper = new StatementWrapper(query, queryParams);

                IList<StatementWrapper> statements = new List<StatementWrapper>();
                statements.Add(stmtWrapper);

                GetQueryRunner().ExecuteTransaction(GetConnection(), statements);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("FK") || e.Message.Contains("REFERENCE"))
                {
                    throw new Exception("Entity cannot be deleted, because is being used.");
                }
                else if (e.Message.Contains("UNIQUE KEY"))
                {
                    throw new Exception("Entity already exists.");
                }
                else
                {
                    throw new Exception("Unable to execute transactions.", e);
                }
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        private string GetVariableName(Field field)
        {
            return "@" + field.Name.ToUpper();
        }

        /// <summary>
        /// Returns the result of the specified aggregated function(s) executed in a SQL Database.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="aggregateInfo">The aggregateInfo data</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The agregated list of entities</returns>
        public IList<Entity> GetAggregateEntities(Entity entity, AggregateInfo aggregateInfo, FilterInfo.SearchType searchType)
        {
            return GetAggregateEntities(entity, aggregateInfo, searchType, null);
        }

        /// <summary>
        /// Returns the result of the specified aggregated function(s) executed in a SQL Database.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="aggregateInfo">The aggregateInfo data</param>
        /// <param name="searchType">The search type</param>
        /// <param name="filter">The filter info</param>
        /// <returns>The agregated list of entities</returns>
        public IList<Entity> GetAggregateEntities(Entity entity, AggregateInfo aggregateInfo, FilterInfo.SearchType searchType, FilterInfo filter)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();
            try
            {
                Entity aggregateEntity = new Entity();
                StatementWrapper stmtWrapper = GetQueryBuilder().BuildAggregateStatement(entity, aggregateInfo, aggregateEntity, searchType, filter);
                LoggerHelper.Debug(stmtWrapper.Query.ToString());                

                ResultSetHandler<IList<Entity>> h = new EntityHandler<Entity>(aggregateEntity);
                list = GetQueryRunner().Query(GetConnection(), stmtWrapper, h);
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
    }
}