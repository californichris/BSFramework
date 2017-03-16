using System.Data.Common;
using BS.Common.Utils;

namespace BS.Common.Dao
{
    /// <summary>
    /// Based class for QueryRunner
    /// </summary>
    public abstract class AbstractQueryRunner
    {
        /// <summary>
        /// Closes the specified DbConnection, DbCommand and DbDataReader.
        /// <para>if one of the arguments is null no action is taken.</para>
        /// </summary>
        /// <param name="conn">an DbConnection</param>
        /// <param name="cmd">an DbCommand</param>
        /// <param name="rd"> an DbDataReader</param>
        protected virtual void Close(DbConnection conn, DbCommand cmd, DbDataReader rd)
        {
            DbUtils.Close(conn, cmd, rd);
        }
    }
}
