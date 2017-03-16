using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Script.Serialization;
using BS.Common.Dao;
using BS.Common.Entities;
using BS.Common.Entities.Page;
using BS.Common.Utils;

namespace BS.Common.Ajax
{
    /// <summary>
    ///  Ajax handlre for al Query related operations
    /// </summary>
    public class QueryHelper : PageInfoBase
    {
        /// <summary>
        /// Creates an Instance of the QueryHelper Ajax class
        /// </summary>
        public QueryHelper()
        {
        }

        /// <summary>
        /// Returns a SELECT statement of the specified page.
        /// <para>If the request user is not part of the App support team an exeception is returned.</para>
        /// <para>The returned statement type depends on the specified page.connName provider</para>
        /// </summary>
        /// <param name="request">the request</param>
        /// <returns>the statement</returns>
        public virtual string GetSelectStatement(HttpRequest request)
        {
            LoggerHelper.Info("Start");            
            try
            {
                if (!CheckPermissions()) return ErrorResponse("User don't have permissions to use this functionality.");
               
                Page page = GetPage(request);
                Entity entity = CreateEntity(request, page);
                FilterInfo filterInfo = CreateFilter(request);

                IQueryBuilder queryBuilder = GetQueryBuilder(page);
                StatementWrapper stmt = null;

                if (filterInfo == null)
                {
                    stmt = queryBuilder.BuildFindEntitiesStatement(entity, filterInfo, GetSearchType(request));
                }
                else
                {
                    stmt = queryBuilder.BuildSelectStatement(entity, filterInfo);
                }


                return CreateStatementResponse(stmt);
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
        }

        /// <summary>
        /// Returns an INSERT statement of the specified page.
        /// <para>If the request user is not part of the App support team an exeception is returned.</para>
        /// <para>The returned statement type depends on the specified page.connName provider</para>
        /// </summary>
        /// <param name="request">the request</param>
        /// <returns>the statement</returns>
        public virtual string GetInsertStatement(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (!CheckPermissions()) return ErrorResponse("User don't have permissions to use this functionality.");

                Page page = GetPage(request);
                Entity entity = CreateEntity(request, page);

                IQueryBuilder queryBuilder = GetQueryBuilder(page);
                StatementWrapper stmt = queryBuilder.BuildInsertStatement(entity);

                return CreateStatementResponse(stmt);
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
        }

        /// <summary>
        /// Returns an UPDATE statement of the specified page.
        /// <para>If the request user is not part of the App support team an exeception is returned.</para>
        /// <para>The returned statement type depends on the specified page.connName provider</para>
        /// </summary>
        /// <param name="request">the request</param>
        /// <returns>the statement</returns>
        public virtual string GetUpdateStatement(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (!CheckPermissions()) return ErrorResponse("User don't have permissions to use this functionality.");

                Page page = GetPage(request);
                Entity entity = CreateEntity(request, page);

                IQueryBuilder queryBuilder = GetQueryBuilder(page);
                StatementWrapper stmt = queryBuilder.BuildUpdateStatement(entity);

                return CreateStatementResponse(stmt);
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
        }

        /// <summary>
        /// Returns a DELETE statement of the specified page.
        /// <para>If the request user is not part of the App support team an exeception is returned.</para>
        /// <para>The returned statement type depends on the specified page.connName provider</para>
        /// </summary>
        /// <param name="request">the request</param>
        /// <returns>the statement</returns>
        public virtual string GetDeleteStatement(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                if (!CheckPermissions()) return ErrorResponse("User don't have permissions to use this functionality.");

                Page page = GetPage(request);
                Entity entity = CreateEntity(request, page);

                IQueryBuilder queryBuilder = GetQueryBuilder(page);
                StatementWrapper stmt = queryBuilder.BuildDeleteStatement(entity);

                return CreateStatementResponse(stmt);
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
        }

        /// <summary>
        /// Returns the proper IQueryBuilder instance, depending on the specified page
        /// </summary>
        /// <param name="page">The page</param>
        /// <returns>The IQueryBuilder instance</returns>
        protected virtual IQueryBuilder GetQueryBuilder(Page page)
        {
            return DbUtils.GetQueryBuilder(page);
        }

        private string CreateStatementResponse(StatementWrapper stmt)
        {
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("Statement", stmt.Query.ToString());
            JavaScriptSerializer ser = new JavaScriptSerializer();

            return ser.Serialize(response);
        }
    }
}
