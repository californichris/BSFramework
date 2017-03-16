using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using BS.Common.Utils;

namespace BS.Common.Dao
{
    /// <summary>
    /// Executes SQL queries with pluggable strategies for handling
    /// ResultSets. <see cref="ResultSetHandler{T}"/>.
    /// 
    /// </summary>
    public class QueryRunner : AbstractQueryRunner
    {
        /// <summary>
        /// Creates a QueryRunner instance 
        /// </summary>
        public QueryRunner() : base()
        {            
        }

        /// <summary>
        /// Execute an SQL INSERT, UPDATE, or DELETE statement, using the specified Connection
        /// </summary>
        /// <param name="conn">The SQL connection.</param>
        /// <param name="query">The SQL statement.</param>
        public void ExecuteNonQuery(DbConnection conn, StringBuilder query)
        {
            CheckNulls(conn, query);
            DbCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = query.ToString();
                cmd.ExecuteNonQuery();
            }
            finally
            {
                Close(conn, cmd, null);
            }
        }

        /// <summary>
        /// Executes the specified SQL statement using the specified connection, and returns the first column of the first row in the result set returned by the query.
        /// <para>Additional columns or rows are ignored.</para>
        /// </summary>
        /// <param name="conn">The SQL connection.</param>
        /// <param name="query">The SQL statement.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public int ExecuteScalar(DbConnection conn, StringBuilder query)
        {
            CheckNulls(conn, query);
            int newId;
            DbCommand cmd = null;
            try
            {                
                cmd = conn.CreateCommand();
                cmd.CommandText = query.ToString();
                newId = int.Parse(cmd.ExecuteScalar().ToString());
            }
            finally
            {
                Close(conn, cmd, null);
            }

            return newId;
        }

        /// <summary>
        /// Executes a list of SQL INSERT, UPDATE, or DELETE queries as a transaction if one
        /// of the statements fails the transaction is rolledback.
        /// </summary>
        /// <param name="conn">The Connection to use to run the queries.</param>
        /// <param name="queries">The list of statements</param>
        public void ExecuteTransaction(DbConnection conn, IList<string> queries)
        {
            ExecuteTransaction(conn, queries, false);
        }

        /// <summary>
        /// Executes a list of SQL INSERT, UPDATE, or DELETE queries as a transaction if one
        /// of the statements fails the transaction is rolledback.
        /// </summary>
        /// <param name="conn">The Connection to use to run the queries.</param>
        /// <param name="queries">The list of statements</param>
        /// <param name="scalar">Flag signaling if the statements will be executing as Scalar.</param>
        public int ExecuteTransaction(DbConnection conn, IList<string> queries, bool scalar)
        {
            CheckNulls(conn, queries);
            DbCommand cmd = null;
            DbTransaction transaction = null;
            int newId = 0;
            try
            {
                cmd = conn.CreateCommand();

                // Start a local transaction.
                transaction = conn.BeginTransaction();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                
                foreach (string query in queries)
                {
                    cmd.CommandText = query;
                    if(scalar) {
                        newId = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    else
                    {
                        cmd.ExecuteNonQuery();
                    }                    
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("Commit Exception Type: {" + ex.GetType() + "}");
                LoggerHelper.Error("  Message: {" + ex.Message + "}");

                try
                {
                    if (transaction != null) transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred 
                    // on the server that would cause the rollback to fail, such as 
                    // a closed connection.
                    LoggerHelper.Error("Rollback Exception Type: {" + ex2.GetType() + "}");
                    LoggerHelper.Error("  Message: {" + ex2.Message + "}");
                }
                LoggerHelper.Error(ex);
                throw ex;
            }
            finally
            {
                Close(conn, cmd, null);
            }

            return newId;
        }

        /// <summary>
        /// Executes the specified SQL statement using the specified connection.
        /// </summary>
        /// <typeparam name="T">The type of object that the handler returns</typeparam>
        /// <param name="conn">The connection to use for the query call.</param>
        /// <param name="query">The SQL statement to execute.</param>
        /// <param name="rsh">The handler used to create the result object from the ResultSet.</param>
        /// <returns>An object generated by the handler.</returns>
        public T Query<T>(DbConnection conn, StringBuilder query, ResultSetHandler<T> rsh)
        {
            CheckNulls(conn, query);
            T result = default(T);

            DbCommand cmd = null;
            DbDataReader rd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = query.ToString();
                rd = cmd.ExecuteReader();
                result = rsh.Handle(rd);
            }
            finally
            {
                Close(conn, cmd, rd);
            }

            return result;
        }

        /// <summary>
        /// Executes the specified statement using the specified connection.
        /// </summary>
        /// <typeparam name="T">The type of object that the handler returns</typeparam>
        /// <param name="conn">The connection to use for the query call.</param>
        /// <param name="stmtWrapper">The statement wrapper containing the query to execute and the statement params.</param>
        /// <param name="rsh">The handler used to create the result object from the ResultSet.</param>
        /// <returns>An object generated by the handler.</returns>
        public T Query<T>(DbConnection conn, StatementWrapper stmtWrapper, ResultSetHandler<T> rsh)
        {
            CheckNulls(conn, stmtWrapper.Query);
            T result = default(T);

            DbCommand cmd = null;
            DbDataReader rd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = stmtWrapper.Query.ToString();
                SetParameters(cmd, stmtWrapper.DBParams);
                rd = cmd.ExecuteReader();
                result = rsh.Handle(rd);
            }
            finally
            {
                Close(conn, cmd, rd);
            }

            return result;
        }


        /// <summary>
        /// Executes the specified statement using the specified connection, and returns the first column of the first row in the result set returned by the query.
        /// <para>Additional columns or rows are ignored.</para>
        /// </summary>
        /// <param name="conn">The SQL connection.</param>
        /// <param name="stmtWrapper">The statement wrapper containing the query to execute and the statement params.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public int ExecuteScalar(DbConnection conn, StatementWrapper stmtWrapper)
        {
            CheckNulls(conn, stmtWrapper.Query);
            int newId;
            DbCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = stmtWrapper.Query.ToString();
                SetParameters(cmd, stmtWrapper.DBParams);

                //This check id for the Oracle Implementation
                //Because the only way to get the new id is has an output param and
                //executing a NonQuery operation
                DBParam idparam;
                if (HasOutputParams(stmtWrapper, out idparam))
                {
                    DbParameter idOutputParam = cmd.CreateParameter();
                    idOutputParam.ParameterName = idparam.ParamName;
                    idOutputParam.DbType = idparam.ParamType;
                    idOutputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(idOutputParam);

                    cmd.ExecuteNonQuery();

                    newId = Convert.ToInt32(idOutputParam.Value);
                }
                else
                {
                    newId = int.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            finally
            {
                Close(conn, cmd, null);
            }

            return newId;
        }

        /// <summary>
        /// Execute an SQL INSERT, UPDATE, or DELETE statement, using the specified Connection
        /// </summary>
        /// <param name="conn">The SQL connection.</param>
        /// <param name="stmtWrapper">The statement wrapper containing the query to execute and the statement params.</param>
        public void ExecuteNonQuery(DbConnection conn, StatementWrapper stmtWrapper)
        {
            CheckNulls(conn, stmtWrapper.Query);
            DbCommand cmd = null;
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = stmtWrapper.Query.ToString();
                SetParameters(cmd, stmtWrapper.DBParams);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                Close(conn, cmd, null);
            }
        }

        /// <summary>
        /// Executes a list of SQL INSERT, UPDATE, or DELETE statements as a transaction if one
        /// of the statements fails the transaction is rolledback.
        /// </summary>
        /// <param name="conn">The Connection to use to run the queries.</param>
        /// <param name="statements">The list of statements</param>
        public void ExecuteTransaction(DbConnection conn, IList<StatementWrapper> statements)
        {
            ExecuteTransaction(conn, statements, false);
        }

        /// <summary>
        /// Executes a list of SQL INSERT, UPDATE, or DELETE statements as a transaction if one
        /// of the statements fails the transaction is rolledback.
        /// </summary>
        /// <param name="conn">The Connection to use to run the queries.</param>
        /// <param name="statements">The list of statements</param>
        /// <param name="scalar">Flag signaling if the statements will be executing as Scalar.</param>
        public int ExecuteTransaction(DbConnection conn, IList<StatementWrapper> statements, bool scalar)
        {
            CheckNulls(conn, statements);
            DbCommand cmd = null;
            DbTransaction transaction = null;
            int newId = 0;
            try
            {
                

                // Start a local transaction.
                transaction = conn.BeginTransaction();
                //cmd.Connection = conn;
                

                foreach (StatementWrapper stmt in statements)
                {
                    cmd = conn.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = stmt.Query.ToString();
                    SetParameters(cmd, stmt.DBParams);
                    if (scalar)
                    {
                        newId = int.Parse(cmd.ExecuteScalar().ToString());
                    }
                    else
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                Console.WriteLine("  Message: {0}", ex.Message);

                try
                {
                    if (transaction != null) transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred 
                    // on the server that would cause the rollback to fail, such as 
                    // a closed connection.
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }

                throw ex;
            }
            finally
            {
                Close(conn, cmd, null);
            }

            return newId;
        }

        /// <summary>
        /// Check if the specified connection and query are nulls
        /// </summary>
        /// <param name="conn">The database connection</param>
        /// <param name="query">The query</param>
        /// <exception cref="System.Exception">Thrown when one of the arguments is null.</exception>
        private void CheckNulls(DbConnection conn, StringBuilder query)
        {
            if (conn == null)
            {
                throw new Exception("Null connection");
            }

            if (query == null)
            {
                throw new Exception("Null query");
            }
        }

        /// <summary>
        /// Check if the specified connection and list of queries are nulls
        /// </summary>
        /// <param name="conn">The database connection</param>
        /// <param name="queries">The queries</param>
        /// <exception cref="System.Exception">Thrown when one of the arguments is null.</exception>
        private void CheckNulls(DbConnection conn, IList<string> queries)
        {
            if (conn == null)
            {
                throw new Exception("Null connection");
            }

            if (queries == null)
            {
                throw new Exception("Null query");
            }

            if (queries != null && queries.Count <= 0)
            {
                throw new Exception("Empty query list");
            }
        }

        /// <summary>
        /// Check if the specified connection and list of StatementWrappers are nulls
        /// </summary>
        /// <param name="conn">The database connection</param>
        /// <param name="statements">The statements</param>
        /// <exception cref="System.Exception">Thrown when one of the arguments is null.</exception>
        private void CheckNulls(DbConnection conn, IList<StatementWrapper> statements)
        {
            if (conn == null)
            {
                throw new Exception("Null connection");
            }

            if (statements == null)
            {
                throw new Exception("Null query");
            }

            if (statements != null && statements.Count <= 0)
            {
                throw new Exception("Empty query list");
            }
        }


        private bool HasOutputParams(StatementWrapper stmtWrapper, out DBParam param)
        {
            IList<DBParam> stmParams = stmtWrapper.DBParams;
            param = ((List<DBParam>)stmParams).Find(x => x.Direction == ParameterDirection.Output);
            if (param == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the DB command parameters 
        /// </summary>
        /// <param name="cmd">The DBCommand</param>
        /// <param name="stmParams">The statement params</param>
        private void SetParameters(DbCommand cmd, IList<DBParam> stmParams)
        {
            for (int i = 0; i < stmParams.Count; i++)
            {
                //Output params will be added outside of this method
                if (stmParams[i].Direction == ParameterDirection.Output) continue;

                DbParameter p = cmd.CreateParameter();
                p.ParameterName = stmParams[i].ParamName;

                if (stmParams[i].IsLike)
                {
                    p.Value = "%" + stmParams[i].ParamValue + "%";
                    p.DbType = DbType.String;
                }
                else
                {
                    p.Value = stmParams[i].ParamValue;
                    p.DbType = stmParams[i].ParamType;
                }
               
                cmd.Parameters.Add(p);
            }
        }
    }
}
