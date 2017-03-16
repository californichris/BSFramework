using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using BS.Common.Dao.Sql;
using BS.Common.Entities;
using BS.Common.Entities.Page;
using BS.Common.Utils;

namespace BS.Common.Dao.Cache
{
    /// <summary>
    ///  IPageInfoDAO implementation that returns the data from an SQL Database 
    ///  but applies a cache mechanism to avoid going to the database if the 
    ///  page configuration has not been updated.
    /// </summary>
    public class PageCacheDAO : BaseDAO, IPageInfoDAO
    {
        private PageSqlDAO sqlDAO;
        private CacheItemPolicy policy = new CacheItemPolicy();

        /// <summary>
        /// Creates an PageCacheDAO instance with the default connection string
        /// </summary>
        public PageCacheDAO()
        {
            sqlDAO = new PageSqlDAO();
            policy.SlidingExpiration = new TimeSpan(1, 0, 0, 0); // entry should be evicted if it has not been accessed in 1 day
        }

        /// <summary>
        /// Creates an PageCacheDAO instance with the specified connection string
        /// </summary>
        /// <param name="connString">The database connection string</param>
        public PageCacheDAO(string connString)
        {
            sqlDAO = new PageSqlDAO(connString);
            policy.SlidingExpiration = new TimeSpan(1, 0, 0, 0); // entry should be evicted if it has not been accessed in 1 day
        }

        /// <summary>
        /// Returns the Page Configuration from the cache if found otherwise returns the configuration from the database
        /// and stores it in the cache for future requests.
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <param name="pageName">The name of the page</param>
        /// <returns>The Page</returns>
        public Page GetPageConfig(String pageId, String pageName)
        {
            LoggerHelper.Info("Start");
            ObjectCache cache = MemoryCache.Default;

            string key = pageId;
            if (String.IsNullOrEmpty(pageId))
            {
                key = pageName;
            }

            Page page = cache[key] as Page;

            if (page == null)
            {
                page = sqlDAO.GetPageConfig(pageId, pageName);
                if (page == null) throw new Exception("Invalid page name.");
                cache.Set(key, page, policy);
            }
            LoggerHelper.Info("End");
            return page;
        }

        /// <summary>
        /// Returns a list of items related to the specified fieldname  from the database.
        /// This method does not performe any cache logic because the field items can change
        /// any time.
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <param name="orderBy">The order in which the list will be returned</param>
        /// <param name="orderType">The order type in which the list will be sorted</param>
        /// <returns>The list of items in the specified order</returns>
        public IList<PageListItem> GetPageListItems(String fieldName, String orderBy, String orderType)
        {
            return sqlDAO.GetPageListItems(fieldName, orderBy, orderType);
        }

        /// <summary>
        /// Returns a list a all page configurations defined in the data base.
        /// This method does not performe any cache logic
        /// </summary>
        /// <returns>The list of page configurations</returns>
        public IList<Page> GetPageList()
        {
            return sqlDAO.GetPageList();
        }

        /// <summary>
        /// Saves the specified page configuration into the data base and refresh the cache with the new configuration.
        /// </summary>
        /// <param name="page">The page to be saved</param>
        public void SavePage(Page page)
        {
            LoggerHelper.Info("Start");
            sqlDAO.SavePage(page);
            page = sqlDAO.GetPageConfig(page.PageId, "");

            ObjectCache cache = MemoryCache.Default;
            if (cache.Contains(page.PageId))
            {
                cache.Set(page.PageId, page, policy);
            }

            if (cache.Contains(page.Name))
            {
                cache.Set(page.Name, page, policy);
            }
            LoggerHelper.Info("End");
        }

        /// <summary>
        /// Deletes the page configuration from the database and from the cache.
        /// </summary>
        /// <param name="page">The page to be deleted</param>
        public void DeletePage(Page page)
        {
            LoggerHelper.Info("Start");
            sqlDAO.DeletePage(page);

            ObjectCache cache = MemoryCache.Default;
            if (cache.Contains(page.PageId))
            {
                cache.Remove(page.PageId);
            }

            if (cache.Contains(page.Name))
            {
                cache.Remove(page.Name);
            }
            LoggerHelper.Info("End");
        }

        /// <summary>
        /// Returns the list of tables in the data base
        /// </summary>
        /// <returns>The list of tables</returns>
        public IList<Entity> GetTables()
        {
            return sqlDAO.GetTables();
        }

        /// <summary>
        /// Returns the list of columns fo the specified table.
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The list of columns</returns>
        public IList<Entity> GetTableColumns(string tableName)
        {
            return sqlDAO.GetTableColumns(tableName);
        }

        /// <summary>
        /// Refresh the page cache.
        /// </summary>
        public void RefreshCache()
        {
            ObjectCache cache = MemoryCache.Default;
            foreach (KeyValuePair<string, object> pair in cache)
            {
                cache.Remove(pair.Key);
            }
        }
    }
}