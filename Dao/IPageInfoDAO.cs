using System.Collections.Generic;
using BS.Common.Entities.Page;
using BS.Common.Entities;

namespace BS.Common.Dao
{
    /// <summary>
    /// This interface define all Page related operations.
    /// </summary>
    public interface IPageInfoDAO : IBaseDAO
    {
        /// <summary>
        /// Returns the Page Configuration from the data source depending on the page id or name.
        /// </summary>
        /// <param name="pageId">The id of the page</param>
        /// <param name="pageName">The name of the page</param>
        /// <returns>The Page</returns>
        Page GetPageConfig(string pageId, string pageName);
        
        /// <summary>
        /// Returns a list of items related to the specified fieldname
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <param name="orderBy">The order in which the list will be returned</param>
        /// <param name="orderType">The order type in which the list will be sorted</param>
        /// <returns>The list of items in the specified order</returns>
        IList<PageListItem> GetPageListItems(string fieldName, string orderBy, string orderType);
        
        /// <summary>
        /// Returns a list a all page configurations defined in the data source
        /// </summary>
        /// <returns>The list of page configurations</returns>
        IList<Page> GetPageList();

        /// <summary>
        /// Saves the specified page configuration into the data source
        /// </summary>
        /// <param name="page">The page to be saved</param>
        void SavePage(Page page);
        
        /// <summary>
        /// Deletes the page configuration from the datasource
        /// </summary>
        /// <param name="page">The page to be deleted</param>
        void DeletePage(Page page);
        
        /// <summary>
        /// Returns the list of tables in the data source
        /// </summary>
        /// <returns>The list of tables</returns>
        IList<Entity> GetTables();
        
        /// <summary>
        /// Returns the list of columns of the specified table.
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The list of columns</returns>
        IList<Entity> GetTableColumns(string tableName);

        void RefreshCache();
    }
}
