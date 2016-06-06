using System;
using System.Collections.Generic;
using BS.Common.Entities;
using System.Data.Common;

namespace BS.Common.Dao
{
    /// <summary>
    /// RowProcessor implementations convert ResultSet rows into various other objects.
    /// Implementations can extend BasicRowProcessor to protect themselves 
    /// from changes to this interface.
    /// </summary>
    public interface RowProcessor
    {
        /// <summary>
        /// Create a Bean from the column values in one ResultSet row.
        /// The ResultSet should be positioned on a valid row before passing it to this method.  
        /// Implementations of this method must not alter the row position of the ResultSet.
        /// </summary>
        /// <typeparam name="T">The type of bean to create</typeparam>
        /// <param name="rs">ResultSet that supplies the bean data</param>
        /// <param name="type">Type Class from which to create the bean instance</param>
        /// <returns>The newly created bean</returns>
        T ToBean<T>(DbDataReader rs, Type type);

        /// <summary>
        /// Create a List of Beans from the column values in all
        /// ResultSet rows.  ResultSet.next() should  
        /// <strong>not</strong> be called before passing it to this method.
        /// </summary>
        /// <typeparam name="T">The type of bean to create</typeparam>
        /// <param name="rs">ResultSet that supplies the bean data</param>
        /// <param name="type">Type Class from which to create the bean instance</param>
        /// <returns>A List of beans with the given type in the order they were returned by the ResultSet.</returns>
        IList<T> ToBeanList<T>(DbDataReader rs, Type type);
    }
}
