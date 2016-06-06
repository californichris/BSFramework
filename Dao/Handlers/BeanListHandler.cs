using System;
using System.Collections.Generic;
using System.Data.Common;

namespace BS.Common.Dao.Handlers
{
    /// <summary>
    /// ResultSetHandler implementation that converts a
    /// ResultSet into a List of bean-style objects.
    /// </summary>
    /// <typeparam name="T">The target bean type</typeparam>
    public class BeanListHandler<T> : ResultSetHandler<IList<T>>
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
        /// Creates a new instance of BeanListHandler.
        /// </summary>
        /// <param name="type">The type that objects returned from handle() are created from.</param>
        public BeanListHandler(Type type)
            : this(type, new BasicRowProcesor())
        {
        }

        /// <summary>
        /// Creates a new instance of BeanListHandler.
        /// </summary>
        /// <param name="type">The type that objects returned from handle() are created from.</param>
        /// <param name="processor">The RowProcessor implementation to use when converting rows into beans.</param>
        public BeanListHandler(Type type, RowProcessor processor)
        {
            this.type = type;
            this.processor = processor;
        }

        /// <summary>
        /// Convert the whole ResultSet into a List of bean-style objects with
        /// the Type given in the constructor.
        /// </summary>
        /// <param name="rs">The ResultSet to handle.</param>
        /// <returns>A List of bean-style objects</returns>
        public IList<T> Handle(DbDataReader rs)
        {
            return processor.ToBeanList<T>(rs, type);
        }
    }
}
