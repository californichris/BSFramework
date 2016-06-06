using System;
using System.Collections.Generic;
using BS.Common.Entities.Page;
using BS.Common.Entities;
using System.Web.Script.Serialization;
using BS.Common.Utils;

namespace BS.Common.Dao.File
{
    /// <summary>
    /// IPageInfoDAO implementation that returns the data from a JSON-formatted text file.
    /// </summary>
    public class PageFileDAO : BaseFileDAO, IPageInfoDAO
    {
        /// <summary>
        /// Creates a PageFileDAO instance
        /// </summary>
        public PageFileDAO()
        {
        }

        /// <summary>
        /// Returns the Page Configuration from a JSON-formatted text file depending on the page id or name.
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <param name="pageName">The name of the page</param>
        /// <returns>The Page</returns>
        public Page GetPageConfig(string pageId, string pageName)
        {
            Page page = null;
            IList<Page> pages = GetPageList();
            foreach (Page p in pages)
            {
                if (!String.IsNullOrEmpty(pageId))
                {
                    if (pageId.Equals(p.PageId))
                    {
                        return p;
                    }
                }
                else
                {
                    if (pageName.Equals(p.Name))
                    {
                        return p;
                    }
                }            
            }

            return page;
        }

        /// <summary>
        /// Returns a list of items related to the specified fieldname from a JSON-formatted text file.
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <param name="orderBy">The order in which the list will be returned</param>
        /// <param name="orderType">The order type in which the list will be sorted</param>
        /// <returns>The list of items in the specified order</returns>
        public IList<PageListItem> GetPageListItems(string fieldName, string orderBy, string orderType)
        {
            LoggerHelper.Info("Start");
            IList<PageListItem> filterItems = new List<PageListItem>();
            
            try
            {
                string json = LoadFile("PageListItem.txt");

                IList<PageListItem> items = (IList<PageListItem>) new JavaScriptSerializer().Deserialize(json, typeof(IList<PageListItem>));
                LoggerHelper.Debug("Done deserializing data.");

                filterItems = ((List<PageListItem>)items).FindAll(i => i.FieldName.Equals(fieldName));
                LoggerHelper.Debug("Done filtering data.");

                ((List<PageListItem>) filterItems).Sort(delegate(PageListItem x, PageListItem y)
                {
                    string xValue = (string)x.GetType().GetField(orderBy).GetValue(x);
                    string yValue = (string)y.GetType().GetField(orderBy).GetValue(y);

                    if ("ASC".Equals(orderBy, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return xValue.CompareTo(yValue);
                    }
                    else
                    {
                        return yValue.CompareTo(xValue);
                    }
                });
                LoggerHelper.Debug("Done sorting data.");
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Not able to fetch PageListItems data.", e);
            }

            LoggerHelper.Info("End");
            return filterItems;
        }

        /// <summary>
        /// Returns a list a all page configurations defined in a JSON-formatted text file.
        /// </summary>
        /// <returns>The list of page configurations</returns>
        public IList<Page> GetPageList()
        {
            LoggerHelper.Info("Start");
            IList<Page> pages = new List<Page>();
            
            try
            {
                string json = LoadFile("Page.txt");               
                pages = (IList<Page>)new JavaScriptSerializer().Deserialize(json, typeof(IList<Page>));
                LoggerHelper.Debug("Done deserializing data.");
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Not able to fetch Page list.", e);
            }
            
            LoggerHelper.Info("End");
            return pages;
        }

        /// <summary>
        /// This method is not implemented.
        /// </summary>
        /// <param name="page">The page to be saved</param>
        public void SavePage(Page page)
        {
            throw new NotImplementedException("This method is not available in offline mode.");
        }


        /// <summary>
        /// This method is not implemented.
        /// </summary>
        /// <param name="page">The page to be deleted</param>
        public void DeletePage(Page page)
        {
            throw new NotImplementedException("This method is not available in offline mode.");
        }

        /// <summary>
        /// Returns the list of tables that exists in the JSON-formatted text file.
        /// This method was implemented to avoid changes in the UI.
        /// </summary>
        /// <returns>The list of tables</returns>
        public IList<Entity> GetTables()
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();

            try
            {
                string json = LoadFile("Tables.txt");
                list = DeserializeEntityList(json);
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Not able to fetch Page list.", e);
            }

            LoggerHelper.Info("End");
            return list;
        }

        /// <summary>
        /// Returns the list of columns of the specified table.
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The list of columns</returns>
        public IList<Entity> GetTableColumns(string tableName)
        {
            throw new NotImplementedException("This method is not available in offline mode.");
        }

        public void RefreshCache()
        {
            throw new NotImplementedException("This method is not available for file implementation.");
        }
    }
}