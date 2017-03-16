using System;
using System.Data.Common;
using BS.Common.Entities.Page;

namespace BS.Common.Dao.Handlers
{
    /// <summary>
    /// ResultSetHandler implementation that converts the first
    /// ResultSet row into a Page Object.
    /// </summary>
    /// <typeparam name="T">The target page type</typeparam>
    public class PageInfoHandler<T> : ResultSetHandler<T>
    {
        /// <summary>
        /// The Type of page produced by this handler.
        /// </summary>
        private Type type;

        /// <summary>
        /// The RowProcessor implementation to use when converting rows into pages.
        /// </summary>
        private RowProcessor processor;

        /// <summary>
        /// Creates a new instance of PageInfoHandler.
        /// </summary>
        /// <param name="type">The type that objects returned from handle() are created from.</param>
        public PageInfoHandler(Type type)
        {
            this.type = type;
            this.processor = new BasicRowProcesor();
        }

        /// <summary>
        /// Convert the whole ResultSet into a Page objects.
        /// </summary>
        /// <param name="rs">The ResultSet to process.</param>
        /// <returns>The Initialize Page objects</returns>
        public T Handle(DbDataReader rs)
        {
            T result = default(T);

            Page page = null;

            bool firstTime = true;
            PageTab prevTab = null;

            while (rs.Read())
            {
                if (firstTime)
                {
                    page = processor.ToBean<Page>(rs, typeof(Page));
                    firstTime = false;
                }

                string tabId = rs["TabId"].ToString();

                if (!String.IsNullOrEmpty(tabId))
                {
                    PageTab currentTab = prevTab;
                    if (prevTab == null || !tabId.Equals(prevTab.TabId))
                    {
                        currentTab = processor.ToBean<PageTab>(rs, typeof(PageTab));
                        page.Tabs.Add(currentTab);
                    }


                    string fieldId = rs["FieldId"].ToString();
                    if (!String.IsNullOrEmpty(fieldId))
                    {
                        PageField field = processor.ToBean<PageField>(rs, typeof(PageField));
                        string columnId = rs["ColumnId"].ToString();
                        if (!String.IsNullOrEmpty(columnId))
                        {
                            PageGridColumn column = processor.ToBean<PageGridColumn>(rs, typeof(PageGridColumn));

                            if (String.IsNullOrEmpty(column.ColumnName))
                            {
                                column.ColumnName = field.FieldName;
                            }

                            if (String.IsNullOrEmpty(column.ColumnLabel))
                            {
                                column.ColumnLabel = field.Label;
                            }

                            page.GridFields.Add(column);
                        }

                        currentTab.Fields.Add(field);
                        prevTab = currentTab;
                    }
                }
            }

            result = (T) Convert.ChangeType(page, type);

            return result;
        }
    }
}