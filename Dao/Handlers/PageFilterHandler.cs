using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using BS.Common.Entities.Page;

namespace BS.Common.Dao.Handlers
{
    /// <summary>
    /// ResultSetHandler implementation that converts the first
    /// ResultSet row into a PageFilter Object.
    /// </summary>
    /// <typeparam name="T">The target page type</typeparam>
    public class PageFilterHandler<T> : ResultSetHandler<T>
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
        public PageFilterHandler(Type type)
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

            PageFilter filter = null;

            bool firstTime = true;
            while (rs.Read())
            {
                if (firstTime)
                {
                    filter = processor.ToBean<PageFilter>(rs, typeof(PageFilter));
                    firstTime = false;
                }

                string fieldId = rs["FilterFieldId"].ToString();
                PageFilterField field = processor.ToBean<PageFilterField>(rs, typeof(PageFilterField));
                filter.Fields.Add(field);
            }

            result = (T) Convert.ChangeType(filter, type);

            return result;
        }
    }
}
