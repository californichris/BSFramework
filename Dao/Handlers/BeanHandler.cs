using System;
using System.Data.Common;

namespace BS.Common.Dao.Handlers
{
    /// <summary>
    /// ResultSetHandler implementation that converts the first
    /// ResultSet row into a Bean-Style Object.
    /// </summary>
    /// <typeparam name="T">The target bean type</typeparam>
    public class BeanHandler<T> : ResultSetHandler<T>
    {
        /// <summary>
        /// The Type of beans produced by this handler.
        /// </summary>
        private Type type;

        /// <summary>
        /// The RowProcessor implementation to use when converting rows into beans.
        /// </summary>
        private RowProcessor processor;

        /// <summary>
        /// Creates a new instance of BeanHandler.
        /// </summary>
        /// <param name="type">The type that objects returned from handle() are created from.</param>
        public BeanHandler(Type type)
            : this(type, new BasicRowProcesor())
        {
        }

        /// <summary>
        /// Creates a new instance of BeanHandler.
        /// </summary>
        /// <param name="type">The type that objects returned from handle() are created from.</param>
        /// <param name="processor">The RowProcessor implementation to use when converting rows into beans.</param>
        public BeanHandler(Type type, RowProcessor processor)
        {
            this.type = type;
            this.processor = processor;
        }

        /// <summary>
        /// Convert the first row of the ResultSet into a bean with the
        /// Type given in the constructor.
        /// </summary>
        /// <param name="rs">ResultSet to process.</param>
        /// <returns>An initialized Bean-Style object or null if there were no rows in the ResultSet.</returns>
        public T Handle(DbDataReader rs)
        {
            T result = default(T);

            if (rs.Read())
            {
                result = processor.ToBean<T>(rs, type);
            }

            return result;
        }
    }
}
