using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceModel;
using System.Text;
using System.Web.Script.Serialization;
using BS.Common.Dao.Sql;
using BS.Common.Entities;
using BS.Common.Entities.Page;
using BS.Common.Utils;

namespace BS.Common.Dao.Service
{
    public class PageInfoServiceDAO : IPageInfoDAO
    {
        /// <summary>
        /// BS Framework Service app name config key
        /// </summary>
        public static readonly string AppNameKey = "BSFrameworkAppName";

        private string AppName = ConfigurationManager.AppSettings[AppNameKey];
        private BSFrameworkService.PageServiceSoapClient client;
        private PageSqlDAO sqlDAO;
        private CacheItemPolicy policy = new CacheItemPolicy();


        public PageInfoServiceDAO()
        {
            client = GetServiceClient();
            sqlDAO = GetPageSqlDAO();
            SetExpirationPolicy();
        }

        protected virtual PageSqlDAO GetPageSqlDAO()
        {
            //TODO: validate if is necesary to used the 
            //factory FactoryUtils.GetDAO(ConfigurationManager.AppSettings["IPageInfoDAO"]);
            return new PageSqlDAO();
        }

        protected virtual BSFrameworkService.PageServiceSoapClient GetServiceClient()
        {
            client = new BSFrameworkService.PageServiceSoapClient();
            return client;
        }

        protected virtual void SetExpirationPolicy()
        {
            policy.SlidingExpiration = new TimeSpan(1, 0, 0, 0); // entry should be evicted if it has not been accessed in 1 day 
        }

        protected CacheItemPolicy GetPolicy()
        {
            return this.policy;
        }

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

        public virtual IList<PageListItem> GetPageListItems(string fieldName, string orderBy, string orderType)
        {
            return sqlDAO.GetPageListItems(fieldName, orderBy, orderType);
        }

        public virtual IList<Entity> GetTables()
        {
            return sqlDAO.GetTables();
        }

        public virtual IList<Entity> GetTableColumns(string tableName)
        {
            return sqlDAO.GetTableColumns(tableName);
        }

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
