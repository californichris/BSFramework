using System.Data.Common;

namespace BS.Common.Utils
{
    /// <summary>
    /// This class consists of static utility methods to help with Database operations.
    /// </summary>
    public static class DbUtils
    {
        /// <summary>
        /// Closes the specified DbConnection, DbCommand and DbDataReader.
        /// <para>if one of the arguments is null no action is taken.</para>
        /// </summary>
        /// <param name="conn">an DbConnection</param>
        /// <param name="cmd">an DbCommand</param>
        /// <param name="rd"> an DbDataReader</param>
        public static void Close(DbConnection conn, DbCommand cmd, DbDataReader rd)
        {
            Close(rd);
            Close(cmd);
            Close(conn);
        }

        /// <summary>
        /// Closes the specified DbConnection.
        /// <para>if one of the arguments is null no action is taken.</para>
        /// </summary>
        /// <param name="conn">an DbConnection</param>
        public static void Close(DbConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
                conn = null;
            }
        }

        /// <summary>
        /// Closes the specified DbCommand.
        /// <para>if one of the arguments is null no action is taken.</para>
        /// </summary>
        /// <param name="cmd">an DbCommand</param>
        public static void Close(DbCommand cmd)
        {
            if (cmd != null)
            {
                cmd.Dispose();
                cmd = null;
            }
        }

        /// <summary>
        /// Closes the specified DbDataReader.
        /// <para>if one of the arguments is null no action is taken.</para>
        /// </summary>
        /// <param name="rd">an DbDataReader</param>
        public static void Close(DbDataReader rd)
        {
            if (rd != null)
            {
                rd.Close();
                rd = null;
            }
        }
    }
}
