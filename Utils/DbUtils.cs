using System;
using System.Configuration;
using System.Data.Common;
using BS.Common.Entities.Page;

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

        /// <summary>
        /// Returns the default IQueryBuilder instance
        /// </summary>
        /// <returns>The IQueryBuilder instance</returns>
        public static IQueryBuilder GetQueryBuilder()
        {
            return GetQueryBuilder("");
        }

        /// <summary>
        /// Returns the proper IQueryBuilder instance, depending on the specified page.connName provider
        /// </summary>
        /// <param name="page">The page containing the connName</param>
        /// <returns>The IQueryBuilder instance</returns>
        public static IQueryBuilder GetQueryBuilder(Page page)
        {
            string connName = "";
            if (page != null && !string.IsNullOrEmpty(page.ConnName))
            {
                connName = page.ConnName;
            }

            return GetQueryBuilder(connName);
        }

        /// <summary>
        /// Returns the proper IQueryBuilder instance, depending on the specified connName provider
        /// </summary>
        /// <param name="connName">The connection string name</param>
        /// <returns>The IQueryBuilder instance, if connName is not specified SQL instance will be returned.</returns>
        public static IQueryBuilder GetQueryBuilder(string connName)
        {
            //getting default
            ConnectionStringSettings connSettings = ConfigurationManager.ConnectionStrings[GetConnectionStringName()];
            if (!string.IsNullOrEmpty(connName))
            {
                connSettings = ConfigurationManager.ConnectionStrings[connName];
                if (connSettings == null)
                {
                    throw new Exception("Invalid connection name [" + connName + "]");
                }
            }

            if (IsOracle(connSettings))
            {
                return new OracleQueryBuilder();
            }
            //TODO: Add else if for future implementations such as MySQL,Postgresql

            return new QueryBuilder();
        }

        /// <summary>
        /// Returns the specified connection string name in the Web.config file
        /// </summary>
        /// <returns>the specified connection string name; otherwise the default conn name EPE.Common.Dao.BaseSqlDAO.DefaultConnString.</returns>
        public static string GetConnectionStringName()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[FactoryUtils.DBConfigParamName]))
            {
                return BS.Common.Dao.BaseSqlDAO.DefaultConnString;
            } else {
                return ConfigurationManager.AppSettings[FactoryUtils.DBConfigParamName];
            }
        }

        /// <summary>
        /// Indicates whether the specified connName provider is Oracle.
        /// </summary>
        /// <param name="connName">The connection name</param>
        /// <returns>returns true if the provider is Oracle, false otherwise</returns>
        public static bool IsOracle(string connName)
        {
            return IsOracle(ConfigurationManager.ConnectionStrings[connName]);
        }

        /// <summary>
        /// Indicates whether the specified connSettings provider is Oracle.
        /// </summary>
        /// <param name="connSettings">The named connection string in the connection strings configuration</param>
        /// <returns>returns true if the provider is Oracle, false otherwise</returns>
        public static bool IsOracle(ConnectionStringSettings connSettings)
        {
            return connSettings.ProviderName.Contains("Oracle");
        }
    }
}
