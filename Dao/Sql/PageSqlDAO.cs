using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BS.Common.Dao.Handlers;
using BS.Common.Entities;
using BS.Common.Entities.Page;
using BS.Common.Utils;

namespace BS.Common.Dao.Sql
{
    /// <summary>
    /// IPageInfoDAO implementation that returns the data from an SQL Database.
    /// </summary>
    public class PageSqlDAO : BaseSqlDAO, IPageInfoDAO
    {
        /// <summary>
        /// Creates a PageSqlDAO instance with the default connection string
        /// </summary>
        public PageSqlDAO():base()
        {
        }

        /// <summary>
        /// Creates a PageSqlDAO instance with the specified connection string
        /// </summary>
        /// <param name="connString">The database connection string</param>
        public PageSqlDAO(string connString) : base(connString)
        {
        }

        /// <summary>
        /// Returns a list a all page configurations defined in the database.
        /// </summary>
        /// <returns>The list of page configurations</returns>
        public virtual IList<Page> GetPageList()
        {
            LoggerHelper.Info("Start");
            IList<Page> list = null;

            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT * FROM Page ORDER BY Name ");
                LoggerHelper.Debug(query.ToString());

                ResultSetHandler<IList<Page>> h = new BeanListHandler<Page>(typeof(Page));
                list = GetQueryRunner().Query(GetConnection(), query, h);
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
        /// Returns a list of pages for the specified AppName
        /// </summary>
        /// <param name="appName">The application name </param>
        /// <returns>the page list</returns>
        public virtual IList<Page> GetPageList(string appName)
        {
            LoggerHelper.Info("Start");
            IList<Page> list = null;
            IList<DBParam> queryParams = new List<DBParam>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT * FROM Page P ");
                query.Append("INNER JOIN PageApp A ON P.PageAppId = A.PageAppId ");
                query.Append("WHERE A.AppName = @p0 ");
                query.Append("ORDER BY Name ");

                queryParams.Add(new DBParam(queryParams, appName, DbType.String));

                LoggerHelper.Debug(query.ToString());
                StatementWrapper wrapper = new StatementWrapper(query, queryParams);

                ResultSetHandler<IList<Page>> h = new BeanListHandler<Page>(typeof(Page));
                list = GetQueryRunner().Query(GetConnection(), wrapper, h);
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
        /// Saves the specified page configuration of the specified application name into the database.
        /// </summary>
        /// <param name="appName">the application name</param>
        /// <param name="page">The page to be saved</param>
        public virtual void SavePage(string appName, Page page)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (string.IsNullOrEmpty(page.PageAppId))
                {
                    StringBuilder query = new StringBuilder("SELECT PageAppId FROM PageApp WHERE AppName = @p0");
                    IList<DBParam> queryParams = new List<DBParam>();
                    queryParams.Add(new DBParam(queryParams, appName, DbType.String));
                    LoggerHelper.Debug(query.ToString());
                    
                    StatementWrapper wrapper = new StatementWrapper(query, queryParams);

                    int pageAppId = GetQueryRunner().ExecuteScalar(GetConnection(), wrapper);
                    page.PageAppId = pageAppId.ToString();
                }

                SavePage(page, false);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to save application page.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            
        }

        /// <summary>
        /// Saves the specified page configuration into the database.
        /// </summary>
        /// <param name="page">The page to be saved</param>
        public virtual void SavePage(Page page)
        {
            SavePage(page, true);
        }
        
        private void SavePage(Page page, bool local)
        {
            LoggerHelper.Info("Start");

            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("DECLARE @PAGE_ID AS INT\n");
                query.Append("DECLARE @TAB_ID AS INT\n");
                query.Append("DECLARE @FIELD_ID AS INT\n");
                query.Append("DECLARE @FILTER_ID AS INT\n");
                query.Append("\n");

                IDictionary<string, string> defaultVals = new Dictionary<string, string>();
                defaultVals["UpdatedDate"] = "GETDATE()";
                defaultVals["PageId"] = "@PAGE_ID";
                defaultVals["TabId"] = "@TAB_ID";
                defaultVals["FieldId"] = "@FIELD_ID";

                if (String.IsNullOrEmpty(page.PageId)) //New Page
                {
                    if (local)
                    {
                        query.Append("INSERT INTO [Page]([Name],[Title],[TableName],[UpdatedBy],[UpdatedDate],[ConnName]) VALUES(");
                        query.Append("'").Append(page.Name).Append("','").Append(page.Title).Append("','").Append(page.TableName).Append("','");
                        query.Append(page.UpdatedBy).Append("',GETDATE(),'").Append(page.ConnName).Append("')").Append('\n');
                    }
                    else
                    {
                        query.Append(GetQueryBuilder().BuildInsertQuery(page, "PageId", defaultVals)).Append('\n');
                    }
                    
                    query.Append("SET @PAGE_ID = scope_identity()\n");
                }
                else
                {
                    if (local)
                    {
                        query.Append("UPDATE [Page] SET [Name] = '").Append(page.Name).Append("', [Title] = '").Append(page.Title);
                        query.Append("', [TableName] = '").Append(page.TableName).Append("', [UpdatedBy] = '").Append(page.UpdatedBy);
                        query.Append("', [UpdatedDate] = GETDATE(), [ConnName] = '").Append(page.ConnName).Append("' ");
                        query.Append("WHERE PageId = ").Append(page.PageId).Append('\n');
                    }
                    else
                    {
                        query.Append(GetQueryBuilder().BuildUpdateQuery(page, "PageId", defaultVals)).Append('\n');
                    }
                    
                    query.Append("SET @PAGE_ID = ").Append(page.PageId).Append("\n");
                    query.Append("DELETE FROM PageGridColumn where PageId = @PAGE_ID\n");
                    query.Append("DELETE FROM [PageFilterField] WHERE [FilterId] IN (SELECT [FilterId] FROM PageFilter WHERE [PageId] = @PAGE_ID)\n");
                    query.Append("DELETE FROM [PageFilter] WHERE [PageId] = @PAGE_ID\n");
                }

                foreach(PageTab tab in page.Tabs) {
                    tab.UpdatedBy = page.UpdatedBy;

                    if (!"1".Equals(tab.UpdatedDate))
                    {
                        if (String.IsNullOrEmpty(tab.TabId))
                        {
                            query.Append(GetQueryBuilder().BuildInsertQuery(tab, "TabId", defaultVals)).Append("\n");
                            query.Append("SET @TAB_ID = scope_identity()\n");
                        }
                        else
                        {
                            query.Append(GetQueryBuilder().BuildUpdateQuery(tab, "TabId", defaultVals)).Append("\n");
                            query.Append("SET @TAB_ID = ").Append(tab.TabId).Append("\n");
                        }
                    }
                    
                    foreach(PageField field in tab.Fields) {
                        field.UpdatedBy = page.UpdatedBy;

                        if (!String.IsNullOrEmpty(field.FieldId) && "1".Equals(field.UpdatedDate))
                        {
                            query.Append("DELETE FROM PageField where FieldId = ").Append(field.FieldId);
                        }
                        else if (String.IsNullOrEmpty(field.FieldId))
                        {
                            query.Append(GetQueryBuilder().BuildInsertQuery(field, "FieldId", defaultVals)).Append('\n');
                            if (field.ColumnInfo != null && !"1".Equals(field.ColumnInfo.UpdatedDate))
                            {
                                query.Append("SET @FIELD_ID = scope_identity()\n");
                            }
                        }
                        else
                        {
                            query.Append(GetQueryBuilder().BuildUpdateQuery(field, "FieldId", defaultVals)).Append('\n');
                            if (field.ColumnInfo != null && !"1".Equals(field.ColumnInfo.UpdatedDate))
                            {
                                query.Append("SET @FIELD_ID = ").Append(field.FieldId).Append("\n");
                            }                            
                        }

                        if (field.ColumnInfo != null && !"1".Equals(field.ColumnInfo.UpdatedDate))
                        {                            
                            PageGridColumn column = field.ColumnInfo;
                            column.UpdatedBy = page.UpdatedBy;

                            query.Append(GetQueryBuilder().BuildInsertQuery(column, "ColumnId", defaultVals)).Append('\n');
                        }
                    }

                    if (!String.IsNullOrEmpty(tab.TabId) && "1".Equals(tab.UpdatedDate))
                    {
                        query.Append("DELETE FROM PageTab where TabId = ").Append(tab.TabId).Append('\n');
                    }
                }

                if (page.Filter != null)
                {
                    query.Append(GetQueryBuilder().BuildInsertQuery(page.Filter, "FilterId", defaultVals)).Append("\n");
                    query.Append("SET @FILTER_ID = scope_identity()\n");
                    
                    defaultVals.Remove("FieldId");
                    defaultVals["FilterId"] = "@FILTER_ID";

                    foreach (PageFilterField field in page.Filter.Fields) {
                        field.UpdatedBy = page.UpdatedBy;
                        query.Append(GetQueryBuilder().BuildInsertQuery(field, "FilterFieldId", defaultVals)).Append('\n');
                    }
                }

                LoggerHelper.Debug(query.ToString());
                IList<String> queries = new List<string>();
                queries.Add(query.ToString());

                int id = GetQueryRunner().ExecuteTransaction(GetConnection(), queries, String.IsNullOrEmpty(page.PageId));
                if (String.IsNullOrEmpty(page.PageId))
                {
                    page.PageId = id.ToString();
                    LoggerHelper.Debug("pageid is null getting new id from db" + page.PageId);
                }                
            }
            catch (Exception e)
            {                
                LoggerHelper.Error(e);
                throw new Exception("Unable to save/update page.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Deletes the page configuration from the database
        /// </summary>
        /// <param name="page">The page to be deleted</param>
        public virtual void DeletePage(Page page)
        {          
            LoggerHelper.Info("Start");
            try
            {
                StringBuilder query = new StringBuilder();
                IList<DBParam> queryParams = new List<DBParam>();

                query.Append("DELETE FROM [PageGridColumn] WHERE [PageId] = @p0");
                query.Append("\n DELETE FROM [PageFilterField] WHERE [FilterId] IN (SELECT [FilterId] FROM PageFilter WHERE [PageId] = @p1)");
                query.Append("\n DELETE FROM [PageField] WHERE [TabId] IN (SELECT [TabId] FROM [PageTab] WHERE [PageId] = @p2 )");
                query.Append("\n DELETE FROM [PageTab] WHERE [PageId] = @p3");
                query.Append("\n DELETE FROM [PageFilter] WHERE [PageId] = @p4");
                query.Append("\n DELETE FROM [Page] WHERE [PageId] = @p5");

                queryParams.Add(new DBParam(queryParams, page.PageId, DbType.Int32));
                queryParams.Add(new DBParam(queryParams, page.PageId, DbType.Int32));
                queryParams.Add(new DBParam(queryParams, page.PageId, DbType.Int32));
                queryParams.Add(new DBParam(queryParams, page.PageId, DbType.Int32));
                queryParams.Add(new DBParam(queryParams, page.PageId, DbType.Int32));
                queryParams.Add(new DBParam(queryParams, page.PageId, DbType.Int32));

                LoggerHelper.Debug(query.ToString());

                StatementWrapper wrapper = new StatementWrapper(query, queryParams);
                GetQueryRunner().ExecuteNonQuery(GetConnection(), wrapper);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to delete page.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }
        }

        /// <summary>
        /// Returns the Page Configuration from a database depending on the page id or name.
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
                StringBuilder query = new StringBuilder();
                IList<DBParam> queryParams = new List<DBParam>();

                query.Append("SELECT P.PageId, P.Name, P.Title, P.TableName, P.ConnName, T.TabId, T.TabName, T.TabOrder, T.Cols, F.FieldId, F.FieldName, ");
                query.Append("F.Label, F.Type, F.Required, F.DropDownInfo, F.Exportable, F.IsId, F.FieldOrder, F.ControlType, F.JoinInfo, ");
                query.Append("F.DBFieldName, F.Insertable, F.Updatable, F.ControlProps, ");
                query.Append("G.ColumnId, G.Visible, G.Searchable, G.Width, G.ColumnName, G.ColumnLabel, G.ColumnOrder ");
                query.Append("FROM Page P ");
                query.Append("LEFT OUTER JOIN PageTab T ON P.PageId = T.PageId ");
                query.Append("LEFT OUTER JOIN PageField F ON T.TabId = F.TabId ");
                query.Append("LEFT OUTER JOIN PageGridColumn G ON P.PageId = G.PageId AND F.FieldId = G.FieldId ");
                
                if (!String.IsNullOrEmpty(pageId))
                {
                    query.Append("WHERE P.PageId = @p0 ");
                    queryParams.Add(new DBParam(queryParams, pageId, DbType.Int32));
                }
                else
                {
                    query.Append("WHERE P.Name = @p0 ");
                    queryParams.Add(new DBParam(queryParams, pageName, DbType.String));
                }
                
                query.Append("ORDER BY T.TabOrder, F.FieldOrder, F.Label");
                LoggerHelper.Debug(query.ToString());

                StatementWrapper wrapper = new StatementWrapper(query, queryParams);
                ResultSetHandler<Page> h = new PageInfoHandler<Page>(typeof(Page));
                page = GetQueryRunner().Query(GetConnection(), wrapper, h);
                page.Filter = GetPageFilter(page.PageId);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get page config.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return page;
        }

        /// <summary>
        /// Returns the Page Configuration from the data base for the specified application name and pageId or pageName
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="pageId">The id of the page</param>
        /// <param name="pageName">The name of the page</param>
        /// <returns>The Page</returns>
        public virtual Page GetPageConfig(string appName, string pageId, string pageName)
        {
            if (string.IsNullOrEmpty(appName))
            {
                LoggerHelper.Warning("Page Application Name is not specified.");
                throw new ArgumentNullException("Page Application Name is not specified.");
            }


            LoggerHelper.Info("Start");
            Page _page = null;

            try
            {
                StringBuilder query = new StringBuilder();
                IList<DBParam> queryParams = new List<DBParam>();

                query.Append("SELECT A.PageAppId, A.AppName, P.PageId, P.Name, P.Title, P.TableName, P.ConnName, T.TabId, T.TabName, T.TabOrder, T.Cols, F.FieldId, F.FieldName, ");
                query.Append("F.Label, F.Type, F.Required, F.DropDownInfo, F.Exportable, F.IsId, F.FieldOrder, F.ControlType, F.JoinInfo, ");
                query.Append("F.DBFieldName, F.Insertable, F.Updatable, F.ControlProps, ");
                query.Append("G.ColumnId, G.Visible, G.Searchable, G.Width, G.ColumnName, G.ColumnLabel, G.ColumnOrder ");
                query.Append("FROM Page P ");
                query.Append("INNER JOIN PageApp A ON A.PageAppId = P.PageAppId ");
                query.Append("LEFT OUTER JOIN PageTab T ON P.PageId = T.PageId ");
                query.Append("LEFT OUTER JOIN PageField F ON T.TabId = F.TabId ");
                query.Append("LEFT OUTER JOIN PageGridColumn G ON P.PageId = G.PageId AND F.FieldId = G.FieldId ");
                query.Append("WHERE A.AppName = @p0 ");
                queryParams.Add(new DBParam(queryParams, appName, DbType.String));

                if (!String.IsNullOrEmpty(pageId))
                {
                    query.Append("AND P.PageId = @p1 ");
                    queryParams.Add(new DBParam(queryParams, pageId, DbType.Int32));
                }
                else
                {
                    query.Append("AND P.Name = @p1 ");
                    queryParams.Add(new DBParam(queryParams, pageName, DbType.String));
                }

                query.Append("ORDER BY T.TabOrder, F.FieldOrder, F.Label");
                LoggerHelper.Debug(query.ToString());

                StatementWrapper wrapper = new StatementWrapper(query, queryParams);
                ResultSetHandler<Page> h = new PageInfoHandler<Page>(typeof(Page));
                _page = GetQueryRunner().Query(GetConnection(), wrapper, h);
                _page.Filter = GetPageFilter(_page.PageId);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get page config.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return _page;
        }

        /// <summary>
        /// Returns the page filter for the specified page
        /// </summary>
        /// <param name="pageId">the page id</param>
        /// <returns>the page filter</returns>
        public virtual PageFilter GetPageFilter(string pageId)
        {
            LoggerHelper.Info("Start");
            PageFilter filter = null;

            try
            {
                StringBuilder query = new StringBuilder();
                IList<DBParam> queryParams = new List<DBParam>();

                query.Append("SELECT PF.FilterId, PF.FilterText, PF.FilterCols, PF.ShowClear, PF.FilterProps, ");
                query.Append("FF.FilterFieldId, FF.FieldId, FF.FilterOrder ");
                query.Append("FROM PageFilter PF ");
                query.Append("INNER JOIN Page P ON PF.PageId = P.PageId ");
                query.Append("INNER JOIN PageFilterField FF ON PF.FilterId = FF.FilterId ");
                query.Append("WHERE P.PageId = @p0 ");
                query.Append("ORDER BY PF.FilterId, FF.FilterOrder");
                LoggerHelper.Debug(query.ToString());

                queryParams.Add(new DBParam(queryParams, pageId, DbType.Int32));

                StatementWrapper wrapper = new StatementWrapper(query, queryParams);
                ResultSetHandler<PageFilter> h = new PageFilterHandler<PageFilter>(typeof(PageFilter));
                filter = GetQueryRunner().Query(GetConnection(), wrapper, h);
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Unable to get page filter.", e);
                //throw new Exception("Unable to get page filter.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return filter;
        }

        /// <summary>
        /// Returns a list of items related to the specified fieldname from a database.
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <param name="orderBy">The order in which the list will be returned</param>
        /// <param name="orderType">The order type in which the list will be sorted</param>
        /// <returns>The list of items in the specified order</returns>
        public virtual IList<PageListItem> GetPageListItems(string fieldName, string orderBy, string orderType)
        {
            LoggerHelper.Info("Start");
            IList<PageListItem> list = null;

            try
            {
                StringBuilder query = new StringBuilder();
                IList<DBParam> queryParams = new List<DBParam>();

                query.Append("SELECT ItemId,FieldName,Value,Text,ShortText,Enable,Selected FROM PageListItem ");
                query.Append("WHERE FieldName = @p0 AND Enable = 1 ");

                if (String.IsNullOrEmpty(orderBy))
                {
                    orderBy = "Text";
                }

                if (String.IsNullOrEmpty(orderType))
                {
                    orderType = "ASC";
                }

                query.Append("ORDER BY ").Append(orderBy).Append(" ").Append(orderType);
                LoggerHelper.Debug(query.ToString());

                queryParams.Add(new DBParam(queryParams, fieldName, DbType.String));

                StatementWrapper wrapper = new StatementWrapper(query, queryParams);
                ResultSetHandler<IList<PageListItem>> h = new BeanListHandler<PageListItem>(typeof(PageListItem));
                list = GetQueryRunner().Query(GetConnection(), wrapper, h);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to fetch " + fieldName  + " list Items.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return list;
        }

        /// <summary>
        /// Returns the list of tables in the database
        /// </summary>
        /// <returns>The list of tables</returns>
        public virtual IList<Entity> GetTables()
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = null;

            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT CASE WHEN SCHEMA_NAME(schema_id) = 'dbo' THEN name ELSE  SCHEMA_NAME(schema_id) + '.' + name END AS [Name] ");
                query.Append("FROM sys.tables WHERE name <> 'sysdiagrams' ");
                query.Append("UNION ");
                query.Append("SELECT CASE WHEN SCHEMA_NAME(schema_id) = 'dbo' THEN name ELSE  SCHEMA_NAME(schema_id) + '.' + name END AS [Name] ");
                query.Append("FROM sys.views ");
                query.Append("ORDER BY 1");
                LoggerHelper.Debug(query.ToString());

                Entity entity = new Entity("Tables");
                entity.SetField(new Field("Name"));

                ResultSetHandler<IList<Entity>> h = new EntityHandler<Entity>(entity);
                list = GetQueryRunner().Query(GetConnection(), query, h);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get tables/views.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return list;
        }

        /// <summary>
        /// Returns the list of columns of the specified table.
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The list of columns</returns>
        public virtual IList<Entity> GetTableColumns(string tableName)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = null;

            try
            {
                string[] tableInfo = tableName.Split('.');
                LoggerHelper.Debug("tableInfo,length = " + tableInfo.Length.ToString());

                StringBuilder query = new StringBuilder();
                IList<DBParam> queryParams = new List<DBParam>();
                query.Append("SELECT COLUMN_NAME AS [Name], IS_NULLABLE AS [Required], DATA_TYPE AS [Type], ");
                query.Append("CHARACTER_MAXIMUM_LENGTH AS [MaxLength], ORDINAL_POSITION AS [Order] ");
                query.Append("FROM INFORMATION_SCHEMA.COLUMNS ");
                query.Append("WHERE TABLE_NAME = @p0 ");
                if (tableInfo.Length == 1)
                {
                    queryParams.Add(new DBParam(queryParams, tableInfo[0], DbType.String));
                }
                else if (tableInfo.Length == 2)
                {
                    query.Append("AND TABLE_SCHEMA = @p1 ");
                    queryParams.Add(new DBParam(queryParams, tableInfo[1], DbType.String));
                    queryParams.Add(new DBParam(queryParams, tableInfo[0], DbType.String));
                } else {
                    queryParams.Add(new DBParam(queryParams, tableName, DbType.String));
                }


                query.Append("ORDER BY TABLE_NAME,ORDINAL_POSITION ");
                
                LoggerHelper.Debug(query.ToString());

                //TODO : Create an entity object
                Entity entity = new Entity("Columns");
                entity.SetField(new Field("Name"));
                entity.SetField(new Field("Required"));
                entity.SetField(new Field("Type"));
                entity.SetField(new Field("MaxLength"));
                entity.SetField(new Field("Order"));

                StatementWrapper wrapper = new StatementWrapper(query, queryParams);
                ResultSetHandler<IList<Entity>> h = new EntityHandler<Entity>(entity);
                list = GetQueryRunner().Query(GetConnection(), wrapper, h);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get table columns.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return list;
        }

        /// <summary>
        /// This method does not applied for database implementations
        /// </summary>
        public void RefreshCache()
        {
            throw new NotImplementedException("This method is not available for sql implementation.");
        }
    }
}