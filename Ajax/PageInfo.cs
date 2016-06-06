using System;
using System.Collections.Generic;
using System.Web;
using BS.Common.Utils;
using System.Web.Script.Serialization;
using BS.Common.Entities;
using BS.Common.Entities.Page;
using BS.Common.Dao;
using System.Text;
using System.Configuration;
using BS.Common.Dao.Sql;

namespace BS.Common.Ajax
{
    /// <summary>
    /// Ajax handlre for al Page related operations
    /// </summary>
    public class PageInfo : AjaxBase
    {
        /// <summary>
        /// Page Id request parameter name
        /// </summary>
        public static readonly string PageIdParam = "pageId";

        /// <summary>
        /// Page Name request parameter name
        /// </summary>
        public static readonly string PageNameParam = "pageName";

        /// <summary>
        /// Entity request parameter name
        /// </summary>
        public static readonly string EntityParam = "entity";

        /// <summary>
        /// Aggregate Info request parameter name
        /// </summary>
        public static readonly string AggregateParam = "aggregateInfo";

        /// <summary>
        /// Creates an Instance of the PageInfo Ajax class
        /// </summary>
        public PageInfo()
        {
        }

        /// <summary>
        /// Returns the PageInfo data source
        /// </summary>
        /// <returns>The Page data source</returns>
        protected virtual IPageInfoDAO GetPageInfoDAO()
        {
            return (IPageInfoDAO) FactoryUtils.GetDAO(ConfigurationManager.AppSettings["IPageInfoDAO"]);
        }

        /// <summary>
        /// Returns the Catalog data source
        /// </summary>
        /// <returns>The Catalog data source</returns>
        protected virtual ICatalogDAO GetCatalogDAO()
        {
            return (ICatalogDAO) FactoryUtils.GetDAO(ConfigurationManager.AppSettings["ICatalogDAO"]);
        }

        #region Entity related methods

        /// <summary>
        /// Returns the list of entities depending on the specified request.
        /// If the response will be filtered if the request contains an entity param or
        /// filter info. If the request contains a csv param the entity list will be
        /// will be return in a csv format.
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <returns>an entity JSON array or a csv text response</returns>
        public virtual string GetPageEntityList(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();
            FilterInfo filterInfo = null;            

            bool csv = IsCSV(request);
            try
            {
                filterInfo = CreateFilter(request);
                list = GetEntityList(request, filterInfo);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            if (csv)
            {
                bool allColumns = !string.IsNullOrEmpty(request.Params["allColumns"]) && bool.Parse(request.Params["allColumns"]) ? true : false;
                return EntityListToCSV(list, allColumns, GetPage(request));
            }
            else
            {
                return CreateEntityListResponse(list, filterInfo);
            }
        }


        protected virtual IList<Entity> GetEntityList(HttpRequest request, FilterInfo filterInfo)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();
            
            try
            {
                Page page = GetPage(request);
                Entity entity = CreateEntity(request, page, IsCSV(request));

                if (entity.GetProperties().Count == 0)
                {
                    if (filterInfo == null)
                    {
                        list = GetCatalogDAO().GetEntities(entity);
                    }
                    else
                    {
                        list = GetCatalogDAO().GetEntities(entity, filterInfo);
                    }
                }
                else
                {
                    if (filterInfo == null)
                    {
                        list = GetCatalogDAO().FindEntities(entity, GetSearchType(request));
                    }
                    else
                    {
                        list = GetCatalogDAO().FindEntities(entity, filterInfo, GetSearchType(request));
                    }
                }
            }            
            finally
            {
                LoggerHelper.Info("End");
            }

            return list;
        }

        protected virtual FilterInfo CreateFilter(HttpRequest request)
        {
            FilterInfo filterInfo = null;
            if (FilterInfo.ContainsFilterInfo(request))
            {
                //Paging will be performed on server side
                if (request.Params["filterInfo"] != null)
                {
                    string json = request.Params["filterInfo"];
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    ser.RegisterConverters(new JavaScriptConverter[] { new FilterInfoConverter() });
                    filterInfo = ser.Deserialize<FilterInfo>(json);
                }
                else
                {
                    filterInfo = new FilterInfo(request);
                }
            }

            return filterInfo;
        }


        /// <summary>
        /// Saves the specified entity.
        /// Obtaines the page and entity info from the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON response</returns>
        public virtual string SavePageEntity(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            Entity entity = null;
            try
            {
                Page page = GetPage(request);
                entity = CreateEntity(request, page);
                GetCatalogDAO().SaveEntity(entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse(entity);
        }

        /// <summary>
        /// Updates the specified entity.
        /// Obtaines the page and entity info from the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON response</returns>
        public virtual string UpdatePageEntity(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            Entity entity = null;
            try
            {
                Page page = GetPage(request);
                entity = CreateEntity(request, page);

                GetCatalogDAO().UpdateEntity(entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse(entity);
        }

        /// <summary>
        /// Updates and then deletes the specified entity.
        /// Obtaines the page and entity info from the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON response</returns>
        public virtual string UpdateDeletePageEntity(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            Entity entity = null;
            try
            {
                Page page = GetPage(request);
                entity = CreateEntity(request, page);

                GetCatalogDAO().UpdateDeleteEntity(entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse(entity);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// Obtaines the page and entity info from the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON response</returns>
        public virtual string DeletePageEntity(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                Page page = GetPage(request);
                Entity entity = CreateEntity(request, page);
                
                GetCatalogDAO().DeleteEntity(entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse();
        }

        /// <summary>
        /// Deletes all entities that match the specified entity properties
        /// Obtaines the page and entity info from the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON response</returns>
        public virtual string DeletePageEntities(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                Page page = GetPage(request);
                Entity entity = CreateEntity(request, page);

                GetCatalogDAO().DeleteEntities(entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse();
        }

        public virtual string ExecuteTransaction(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                string json = request.Params["entities"];
                LoggerHelper.Debug("entities = " + json);
                List<Dictionary<string, string>> entities = new List<Dictionary<string,string>>();
                if (!String.IsNullOrEmpty(json))
                {
                    entities = new JavaScriptSerializer().Deserialize<List<Dictionary<string, string>>>(json);
                    List<TransOperation> operations = new List<TransOperation>();

                    foreach (Dictionary<string, string> props in entities)
                    {
                        Entity entity = new Entity();
                        entity.SetProperties(props);
                        string pageName = entity.GetProperty("PageName");
                        string operType = entity.GetProperty("OperationType");

                        if (string.IsNullOrEmpty(pageName))
                        {
                            throw new ArgumentNullException("PageName must be specified in all entities.");
                        }

                        if (string.IsNullOrEmpty(operType))
                        {
                            throw new ArgumentNullException("OperationType must be specified in all entities.");
                        }
                        
                        entity.GetProperties().Remove("PageName");
                        entity.GetProperties().Remove("OperationType");

                        Page page = GetPageInfoDAO().GetPageConfig("", pageName);
                        entity.SetTableName(page.TableName);
                        EntityUtils.SetEntityFields(page, entity, false);

                        TransOperation operation = new TransOperation(operType, entity);
                        operations.Add(operation);
                    }

                    GetCatalogDAO().ExecuteTransaction(operations);
                }
                else
                {
                    throw new ArgumentNullException("No operations found in request.");
                }

                
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse();
        }

        public virtual string GetAggreateEntities(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();
            try
            {
                if (string.IsNullOrEmpty(request.Params[AggregateParam])) throw new Exception("aggregate info can not be null.");

                Page page = GetPage(request);                
                
                //Creating entity
                Entity entity = CreateEntity(request, page);                

                string json = request.Params[AggregateParam];
                LoggerHelper.Debug("AggregateInfo = " + json);
                AggregateInfo aggregateInfo = (AggregateInfo) new JavaScriptSerializer().Deserialize(json, typeof(AggregateInfo));
                list = GetCatalogDAO().GetAggregateEntities(entity, aggregateInfo, GetSearchType(request), CreateFilter(request));
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return CreateEntityListResponse(list);
        }

        #endregion

        #region Page Config related methods

        /// <summary>
        /// Returns the configuration of the requested page.
        /// </summary>
        /// <param name="request">The HttpRequest from where the params will be obtained.</param>
        /// <returns>JSON response with the specified page configuration.</returns>
        public virtual string GetPageConfig(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            string json = "";
            try
            {
                Page page = GetPage(request);

                JavaScriptSerializer ser = new JavaScriptSerializer();
                ser.MaxJsonLength = int.MaxValue;
                json = ser.Serialize(page);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return json;
        }

        /// <summary>
        /// Returns a list of all the pages configured.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON array of the pages</returns>
        public virtual string GetPageList(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            IList<Page> list = new List<Page>();
            try
            {
                list = GetPageInfoDAO().GetPageList();
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return ListResponse(list);
        }

        /// <summary>
        /// Saves the specified page configuration.
        /// Obtaines the page info from the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON response</returns>
        public virtual string SavePage(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            Page page = new Page();
            try
            {
                string json = request.Params[EntityParam];
                LoggerHelper.Debug("entity = " + json);
                page = (Page) new JavaScriptSerializer().Deserialize(json, typeof(Page));
                page.UpdatedBy = GetCurrentUserName();
                GetPageInfoDAO().SavePage(page);

                //PageSqlDAO dao = new PageSqlDAO("ServiceDB");
                //dao.SavePage("CMSLite", page);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse(page.PageId);
        }

        /// <summary>
        /// Deletes the specified page configuration.
        /// Obtaines the page info from the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON response</returns>
        public virtual string DeletePage(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                string json = request.Params[EntityParam];
                LoggerHelper.Debug("entity = " + json);
                Page page = (Page) new JavaScriptSerializer().Deserialize(json, typeof(Page));
                page.UpdatedBy = GetCurrentUserName();
                GetPageInfoDAO().DeletePage(page);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse();
        }

        /// <summary>
        /// Returns a list of tables
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>an array JSON response</returns>
        public virtual string GetTables(HttpRequest request)
        {
            IList<Entity> list = new List<Entity>();
            LoggerHelper.Info("Start");
            try
            {
                list = GetPageInfoDAO().GetTables();
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return ListResponse(list);
        }

        /// <summary>
        /// Returns a list of columns of the specified table
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>an array JSON response</returns>
        public virtual string GetTableColumns(HttpRequest request)
        {
            IList<Entity> list = new List<Entity>();
            LoggerHelper.Info("Start");
            try
            {
                list = GetPageInfoDAO().GetTableColumns(request.Params["tableName"]);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return ListResponse(list);
        }

        /// <summary>
        /// Returns a JSON array of items related to the specified field name.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>A JSON array</returns>
        public virtual string GetPageListItems(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            IList<PageListItem> list = new List<PageListItem>();
            try
            {
                list = GetPageInfoDAO().GetPageListItems(request.Params["fieldName"], request.Params["orderBy"], request.Params["orderType"]);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return ListResponse(list);
        }

        public string RefreshCache(HttpRequest request)
        {
            GetPageInfoDAO().RefreshCache();

            return SuccessResponse();
        }

        # endregion

        /// <summary>
        /// Determines the search type of a request
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <returns>The SearchType</returns>
        protected virtual FilterInfo.SearchType GetSearchType(HttpRequest request)
        {
            if (FilterInfo.ContainsFilterInfo(request))
            {
                return (!string.IsNullOrEmpty(request.Params["searchType"]) && "or".Equals(request.Params["searchType"], StringComparison.CurrentCultureIgnoreCase)) ?
                                FilterInfo.SearchType.OR : FilterInfo.SearchType.AND;
            }

            return (!string.IsNullOrEmpty(request.Params["searchType"]) && "and".Equals(request.Params["searchType"], StringComparison.CurrentCultureIgnoreCase)) ?
                FilterInfo.SearchType.AND : FilterInfo.SearchType.OR;
        }        
        
        /// <summary>
        /// Returns the page configuration from the data source
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <returns>The page config</returns>
        private Page GetPage(HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Params[PageIdParam]) && string.IsNullOrEmpty(request.Params[PageNameParam])) throw new Exception("pageName can not be null.");

            return GetPageInfoDAO().GetPageConfig(request.Params[PageIdParam], request.Params[PageNameParam]);
        }
    }
}