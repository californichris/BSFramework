using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BS.Common.Dao.Handlers;
using BS.Common.Entities;
using BS.Common.Entities.Page;
using BS.Common.Utils;

namespace BS.Common.Dao.Oracle
{

    /// <summary>
    /// IPageInfoDAO implementation that returns the data from an Oracle Database.
    /// </summary>
    public class PageOracleDAO : BaseSqlDAO, IPageInfoDAO
    {
        /// <summary>
        /// Creates a PageOracleDAO instance with the default connection string
        /// </summary>
        public PageOracleDAO():base()
        {
        }

        /// <summary>
        /// Creates a PageOracleDAO instance with the specified connection string
        /// </summary>
        /// <param name="connString">The database connection string</param>
        public PageOracleDAO(string connString) : base(connString)
        {
        }

        /// <summary>
        /// Returns a list a all page configurations defined in the database.
        /// </summary>
        /// <returns>The list of page configurations</returns>
        public virtual IList<Page> GetPageList()
        {
            throw new NotImplementedException("Not supported for Oracle.");
        }

        /// <summary>
        /// Returns a list of pages for the specified AppName
        /// </summary>
        /// <param name="appName">The application name </param>
        /// <returns>the page list</returns>
        public virtual IList<Page> GetPageList(string appName)
        {
            throw new NotImplementedException("Not supported for Oracle.");
        }

        /// <summary>
        /// Saves the specified page configuration of the specified application name into the database.
        /// </summary>
        /// <param name="appName">the application name</param>
        /// <param name="page">The page to be saved</param>
        public virtual void SavePage(string appName, Page page)
        {
            throw new NotImplementedException("Not supported for Oracle.");
        }

        /// <summary>
        /// Saves the specified page configuration into the database.
        /// </summary>
        /// <param name="page">The page to be saved</param>
        public virtual void SavePage(Page page)
        {
            throw new NotImplementedException("Not supported for Oracle.");
        }

        private void SavePage(Page page, bool local)
        {
            throw new NotImplementedException("Not supported for Oracle.");
        }

        /// <summary>
        /// Deletes the page configuration from the database
        /// </summary>
        /// <param name="page">The page to be deleted</param>
        public virtual void DeletePage(Page page)
        {
            throw new NotImplementedException("Not supported for Oracle.");
        }

        /// <summary>
        /// Returns the Page Configuration from a database depending on the page id or name.
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <param name="pageName">The name of the page</param>
        /// <returns>The Page</returns>
        public virtual Page GetPageConfig(string pageId, string pageName)
        {
            throw new NotImplementedException("Not supported for Oracle.");
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
            throw new NotImplementedException("Not supported for Oracle.");
        }

        /// <summary>
        /// Returns the page filter for the specified page
        /// </summary>
        /// <param name="pageId">the page id</param>
        /// <returns>the page filter</returns>
        public virtual PageFilter GetPageFilter(string pageId)
        {
            throw new NotImplementedException("Not supported for Oracle.");
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
                query.Append("WHERE FieldName = :p0 AND Enable = 1 ");

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
                throw new Exception("Unable to fetch " + fieldName + " list Items.", e);
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
                query.Append("SELECT OWNER || '.' || TABLE_NAME AS \"Name\" FROM ALL_TABLES  WHERE TABLESPACE_NAME NOT IN ('SYSTEM','SYSAUX','TOOLS') \n");
                query.Append("UNION \n");
                query.Append("SELECT OWNER || '.' || VIEW_NAME AS  \"Name\" FROM ALL_VIEWS \n");
                query.Append("WHERE OWNER NOT IN (SELECT DISTINCT(OWNER) FROM ALL_TABLES WHERE TABLESPACE_NAME IN ('SYSTEM','SYSAUX','TOOLS')) \n");
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
                query.Append("SELECT COLUMN_NAME AS \"Name\",  CASE WHEN NULLABLE = 'Y' THEN 'YES' ELSE 'NO' END AS \"Required\", \n");
                query.Append("DATA_TYPE AS \"Type\", DATA_LENGTH AS \"MaxLength\", COLUMN_ID AS \"Order\" \n");
                query.Append("FROM ALL_TAB_COLUMNS \n");
                query.Append("WHERE OWNER = :p0 AND TABLE_NAME = :p1 \n");
                query.Append("ORDER BY TABLE_NAME,COLUMN_ID ");

                queryParams.Add(new DBParam(":p0", tableInfo[0], DbType.String));
                queryParams.Add(new DBParam(":p1", tableInfo[1], DbType.String));

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
            throw new NotImplementedException("This method is not available for sql implementations.");
        }

    }
}
