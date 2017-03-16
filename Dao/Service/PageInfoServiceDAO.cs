using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using System.Web.Script.Serialization;
using BS.Common.Dao.Oracle;
using BS.Common.Dao.Sql;
using BS.Common.Entities;
using BS.Common.Entities.Page;
using BS.Common.Utils;

namespace BS.Common.Dao.Service
{
    /// <summary>
    /// IPageInfoDAO implementation that returns all page config operarions from the EPEFramework srvice
    /// and all entity operations from the specified datasource in the page.connName
    /// </summary>
    public class PageInfoServiceDAO : BaseDAO, IPageInfoDAO
    {
        /// <summary>
        /// BS Framework Service app name config key
        /// </summary>
        public static readonly string AppNameKey = "BSFrameworkAppName";

        private string AppName = ConfigurationManager.AppSettings[AppNameKey];
        private BSFrameworkService.PageServiceSoapClient client;
        private IPageInfoDAO sqlDAO;
        private CacheItemPolicy policy = new CacheItemPolicy();

        /// <summary>
        /// Return a default instance of PageInfoServiceDAO
        /// </summary>
        public PageInfoServiceDAO():this("")
        {
        }

        /// <summary>
        /// Returns an instance of the PageInfoServiceDAO using the specified connString
        /// </summary>
        /// <param name="connString">the connection string</param>
        public PageInfoServiceDAO(string connString)
        {
            client = GetServiceClient();
            sqlDAO = GetPageSqlDAO(connString);
            SetExpirationPolicy();
        }

        /// <summary>
        /// returns a IPageInfoDAO instance
        /// </summary>
        /// <returns>the IPageInfoDAO</returns>
        protected virtual IPageInfoDAO GetPageSqlDAO()
        {
            return (IPageInfoDAO) GetPageSqlDAO(null);
        }

        /// <summary>
        /// returns the proper IPageInfoDAO instance depending on the connString provider
        /// </summary>
        /// <param name="connString">the conneccion string</param>
        /// <returns>the IPageInfoDAO</returns>
        protected virtual IPageInfoDAO GetPageSqlDAO(string connString)
        {
            if (!string.IsNullOrEmpty(connString) && DbUtils.IsOracle(connString))
            {
                return (IPageInfoDAO) new PageOracleDAO(connString);
            }
            else
            {
                return (IPageInfoDAO) new PageSqlDAO(connString);
            }
        }

        /// <summary>
        /// Returns an instance of the EPEFramework service
        /// </summary>
        /// <returns>the EPEFrameworkService client</returns>
        protected virtual BSFrameworkService.PageServiceSoapClient GetServiceClient()
        {
            client = new BSFrameworkService.PageServiceSoapClient();
            return client;
        }

        /// <summary>
        /// sets the cache policy expiration policy
        /// </summary>
        protected virtual void SetExpirationPolicy()
        {
            policy.SlidingExpiration = new TimeSpan(1, 0, 0, 0); // entry should be evicted if it has not been accessed in 1 day 
        }

        /// <summary>
        /// returns the cache policyh
        /// </summary>
        /// <returns></returns>
        protected CacheItemPolicy GetPolicy()
        {
            return this.policy;
        }

        /// <summary>
        /// Returns a list of pages from the EPEFrameworkService defined for the specified AppName
        /// </summary>
        /// <returns>the page list</returns>
        public virtual IList<Page> GetPageList() {
            LoggerHelper.Info("Start");
            IList<Page> list = null;

            try
            {            
                string json = client.GetPageList(AppName);
                list = (IList<Page>)new JavaScriptSerializer().Deserialize(json, typeof(IList<Page>));
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get page list.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return list;        
        }

        /// <summary>
        /// Saves the specified page in the EPEFrameworkService
        /// </summary>
        /// <param name="page">the page to be saved</param>
        public virtual void SavePage(Page page) {
            LoggerHelper.Info("Start");
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                ser.MaxJsonLength = int.MaxValue;
                string json = ser.Serialize(page);

                client.SavePage(AppName, json);

                json = client.GetPageConfig(AppName, "", page.Name);
                Page _page = (Page) new JavaScriptSerializer().Deserialize(json, typeof(Page));
                page.PageId = _page.PageId;

                ObjectCache cache = MemoryCache.Default;
                if (cache.Contains(_page.PageId))
                {
                    cache.Set(_page.PageId, _page, policy);
                }

                if (cache.Contains(_page.Name))
                {
                    cache.Set(_page.Name, _page, policy);
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to save page configuration.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Deletes the specified page from the EPEFrameworkService
        /// </summary>
        /// <param name="page">the page to be saved</param>
        public virtual void DeletePage(Page page)
        {
            LoggerHelper.Info("Start");
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                ser.MaxJsonLength = int.MaxValue;
                string json = ser.Serialize(page);

                client.DeletePage(json);

                ObjectCache cache = MemoryCache.Default;
                if (cache.Contains(page.PageId))
                {
                    cache.Remove(page.PageId);
                }

                if (cache.Contains(page.Name))
                {
                    cache.Remove(page.Name);
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to delete page configuration.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

        }

        /// <summary>
        /// Returns the Page Configuration from the EPEFrameworkService depending on the page id or name.
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <param name="pageName">The name of the page</param>
        /// <returns>The Page</returns>
        public virtual Page GetPageConfig(string pageId, string pageName)
        {
            LoggerHelper.Info("Start");
            Page page = null;
            try
            {
                ObjectCache cache = MemoryCache.Default;

                string key = pageId;
                if (String.IsNullOrEmpty(pageId))
                {
                    key = pageName;
                }

                page = cache[key] as Page;

                if (page == null)
                {
                    string json = client.GetPageConfig(AppName, pageId, pageName);
                    page = (Page) new JavaScriptSerializer().Deserialize(json, typeof(Page));

                    if (page == null) throw new Exception("Invalid page name.");
                    cache.Set(key, page, policy);
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get page configuration.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return page;
        }

        /// <summary>
        /// Returns the list of pagelistitems from the specified field order by the specified orderby and ordertype direction
        /// </summary>
        /// <param name="fieldName">the field</param>
        /// <param name="orderBy">the order by field</param>
        /// <param name="orderType">the order direction</param>
        /// <returns>the list of page list items</returns>
        public virtual IList<PageListItem> GetPageListItems(string fieldName, string orderBy, string orderType)
        {
            return sqlDAO.GetPageListItems(fieldName, orderBy, orderType);
        }

        /// <summary>
        /// Returns the list of tables
        /// </summary>
        /// <returns>the list of tables</returns>
        public virtual IList<Entity> GetTables()
        {
            return sqlDAO.GetTables();
        }

        /// <summary>
        /// Returns the list of table columns from the specified table name
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <returns>The list of columns</returns>
        public virtual IList<Entity> GetTableColumns(string tableName)
        {
            return sqlDAO.GetTableColumns(tableName);
        }

        /// <summary>
        /// Refresh the list of cache pages
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
