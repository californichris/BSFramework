using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using BS.Common.Dao.Sql;
using BS.Common.Entities;
using BS.Common.Utils;

namespace BS.Common.Dao.Cache
{
    public class CatalogCacheDAO : BaseSqlDAO, ICatalogDAO
    {
        private static ObjectCache cache = new MemoryCache("CatalogCache");
        private CatalogSqlDAO sqlDAO;
        private CacheItemPolicy policy = new CacheItemPolicy();
        private string CacheTables = ConfigurationManager.AppSettings["CatalogCacheTables"];

        public CatalogCacheDAO()
        {
            sqlDAO = new CatalogSqlDAO();
            policy.SlidingExpiration = new TimeSpan(0, 8, 0, 0);
        }


        public CatalogCacheDAO(string connString)
        {
            sqlDAO = new CatalogSqlDAO(connString);
            policy.SlidingExpiration = new TimeSpan(0, 8, 0, 0);
        }

        public void RefreshCache()
        {
            foreach (KeyValuePair<string, object> pair in cache)
            {
                if (IsTableToCache(pair.Key))
                {
                    cache.Remove(pair.Key);
                }
            }
        }

        public IList<Entity> GetEntities(Entity entity)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();

            try
            {
                string key = entity.GetTableName();
                if (IsTableToCache(key))
                {
                    list = cache[key] as IList<Entity>;
                    if (list == null)
                    {
                        list = sqlDAO.GetEntities(entity);
                        cache.Set(key, list, policy);
                    }
                }
                else
                {
                    list = sqlDAO.GetEntities(entity);
                }
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


        public IList<Entity> GetEntities(Entity entity, FilterInfo filter)
        {
            LoggerHelper.Info("Start");

            IList<Entity> list = new List<Entity>();
            try
            {
                ///TODO: Put cache logic here
                list = sqlDAO.GetEntities(entity, filter);
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

        public IList<Entity> FindEntities(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();

            try
            {
                string key = entity.GetTableName();
                if (IsTableToCache(key))
                {
                    List<Entity> _list = (List<Entity>)GetEntities(entity);
                    list = _list;
                    if (filter == null)
                    {
                        //TODO: implement LIST and RANGE and NOT

                        IDictionary<string, string> props = entity.GetProperties(); //store original search request
                        if (entity.GetProperties().Count > 0)
                        {

                            if (searchType == FilterInfo.SearchType.AND)
                            {
                                foreach (KeyValuePair<string, string> pair in entity.GetProperties())
                                {
                                    if (!string.IsNullOrEmpty(pair.Value))
                                    {//
                                        list = _list.FindAll(ent => ent.GetProperty(pair.Key).Equals(pair.Value));
                                    }
                                }
                            }
                            else
                            {
                                list = _list.FindAll(ent => OrFilter(ent, entity));
                            }
                        }
                    }
                    else
                    {
                        //TODO: implment with filter not null
                    }
                }
                else
                {
                    list = sqlDAO.FindEntities(entity, filter, searchType);
                }
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


        public IList<Entity> FindEntities(Entity entity, FilterInfo.SearchType searchType)
        {
            return FindEntities(entity, null, searchType);
        }


        public IList<Entity> FindEntities(Entity entity)
        {
            return FindEntities(entity, FilterInfo.SearchType.OR);
        }


        public void SaveEntity(Entity entity)
        {
            sqlDAO.SaveEntity(entity);

            if (IsTableToCache(entity.GetTableName()))
            {
                cache.Remove(entity.GetTableName());
            }
        }

        /// <summary>
        /// Deletes the specified entity from an SQL database.
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        public void DeleteEntity(Entity entity)
        {
            sqlDAO.DeleteEntity(entity);
            if (IsTableToCache(entity.GetTableName()))
            {
                cache.Remove(entity.GetTableName());
            }
        }


        public void UpdateDeleteEntity(Entity entity)
        {
            sqlDAO.UpdateDeleteEntity(entity);
            if (IsTableToCache(entity.GetTableName()))
            {
                cache.Remove(entity.GetTableName());
            }
        }


        public void UpdateEntity(Entity entity)
        {
            sqlDAO.UpdateEntity(entity);
            if (IsTableToCache(entity.GetTableName()))
            {
                cache.Remove(entity.GetTableName());
            }
        }


        public void DeleteEntities(Entity entity)
        {
            sqlDAO.DeleteEntities(entity);
            if (IsTableToCache(entity.GetTableName()))
            {
                cache.Remove(entity.GetTableName());
            }
        }

        public void ExecuteTransaction(List<TransOperation> operations)
        {
            sqlDAO.ExecuteTransaction(operations);
            //TODO: add logic to remove from cache each operation         
        }

        public IList<Entity> GetAggregateEntities(Entity entity, AggregateInfo aggregateInfo, FilterInfo.SearchType searchType)
        {
            return sqlDAO.GetAggregateEntities(entity, aggregateInfo, searchType);
            //TODO: implement logic to performe the aggregate operation if the table is in the cache
        }

        public IList<Entity> GetAggregateEntities(Entity entity, AggregateInfo aggregateInfo, FilterInfo.SearchType searchType, FilterInfo filter)
        {
            return sqlDAO.GetAggregateEntities(entity, aggregateInfo, searchType, filter);
        }

        private bool IsTableToCache(string sName)
        {
            if (string.IsNullOrEmpty(CacheTables)) return false;
            foreach (string tablename in CacheTables.Split(','))
            {
                if (sName == tablename)
                {
                    return true;
                }
            }

            return false;
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
