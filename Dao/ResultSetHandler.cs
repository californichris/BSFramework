using System.Data.Common;

namespace BS.Common.Dao
{
    /// <summary>
    /// Implementations of this interface convert ResultSets into other objects.
    /// </summary>
    /// <typeparam name="T">The target type the input ResultSet will be converted to.</typeparam>
    public interface ResultSetHandler<T>
    {
        /// <summary>
        /// Turn the ResultSet into an Object.
        /// </summary>
        /// <param name="rs">The ResultSet to handle.  It has not been touched before being passed to this method.</param>
        /// <returns>An Object initialized with ResultSet data.</returns>
        T Handle(DbDataReader rs);
    }
}
