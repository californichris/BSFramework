using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using BS.Common.Dao;
using BS.Common.Entities.Page;
using BS.Common.Utils;

namespace BS.Common.Ajax
{
    /// <summary>
    /// Base class for all PageInfo implementations
    /// </summary>
    public abstract class PageInfoBase : AjaxBase
    {
        /// <summary>
        /// Validates that the user requesting the data is part of the applications team.
        /// 
        /// There are some operations that are only allow to members of the applications team, such as
        /// delete a config, get a select statement, etc.
        /// </summary>
        /// <returns>true if the user is partof the team, false otherwise.</returns>
        protected virtual bool CheckPermissions()
        {
            return true;//System.Web.HttpContext.Current.User.IsInRole(@"EPECAD\GG_APPLICATIONS_USERS");
        }

        /// <summary>
        /// Returns the PageInfo data source
        /// </summary>
        /// <returns>The Page data source</returns>
        protected virtual IPageInfoDAO GetPageInfoDAO()
        {
            return GetPageInfoDAO("");
        }

        /// <summary>
        /// Returns a IPageInfo instance with the specified connName
        /// </summary>
        /// <param name="connName">the connection name</param>
        /// <returns>an IPageInfo instance</returns>
        protected virtual IPageInfoDAO GetPageInfoDAO(string connName)
        {
            return (IPageInfoDAO) FactoryUtils.GetDAO(ConfigurationManager.AppSettings["IPageInfoDAO"], connName);
        }

        /// <summary>
        /// Returns a Page instance using the request to get the pageName or pageId attributes.
        /// </summary>
        /// <param name="request">the httprequest</param>
        /// <returns>the Page instance</returns>
        protected virtual Page GetPage(HttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Params[PageInfo.PageIdParam]) && string.IsNullOrEmpty(request.Params[PageInfo.PageNameParam])) throw new Exception("pageName can not be null.");

            return GetPageInfoDAO().GetPageConfig(request.Params[PageInfo.PageIdParam], request.Params[PageInfo.PageNameParam]);
        }

        /// <summary>
        /// Determines the searchType requested by the user from the request.
        /// </summary>
        /// <param name="request">the httprequest</param>
        /// <returns>the searchType specified by the user, OR otherwise</returns>
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
        /// Creates a FilterInfo instance from the httprequest.
        /// </summary>
        /// <param name="request">the httprequest</param>
        /// <returns>The FilterInfo instance, null if the filterInfo param is not specified.</returns>
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
    }
}
