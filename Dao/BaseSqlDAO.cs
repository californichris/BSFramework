using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using BS.Common.Entities;
using BS.Common.Utils;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace BS.Common.Dao
{
    /// <summary>
    /// Base class for SQL DAO Objects
    /// </summary>
    public class BaseSqlDAO : BaseDAO
    {
        /// <summary>
        /// Default connection string name
        /// </summary>
        public static readonly string DefaultConnString = "DBConnString";

        private string ConnString;
        private QueryRunner queryRunner;
        private IQueryBuilder queryBuilder;

        /// <summary>
        /// Default constructor for all SQL DAO implementations
        /// </summary>
        public BaseSqlDAO() : this(DefaultConnString)
        {
        }

        /// <summary>
        /// Constructs a BaseSqlDAO instance with the specified connString
        /// </summary>
        /// <param name="connString">The connection string</param>
        public BaseSqlDAO(string connString) : this(connString, null)
        {      
        }

        /// <summary>
        /// Constructs a BaseSqlDAO instance with the specified connString and queryRunner
        /// </summary>
        /// <param name="connString">The connString</param>
        /// <param name="queryRunner">The queryRunner</param>
        public BaseSqlDAO(string connString, QueryRunner queryRunner) : this(connString, queryRunner, null)
        {
        }

        /// <summary>
        /// Constructs a BaseSqlDAO instance with the specified connString, queryRunner and queryBuilder,
        /// if the connString is nor provided DBConnString will be used as default,
        /// if the queryRunner is not provided <see cref="T:BS.Common.Dao.QueryRunner"/> will be used,
        /// if the queryBuilder is not provided SQL implementation will be used as default.
        /// </summary>
        /// <param name="connString">The connString</param>
        /// <param name="queryRunner">The queryRunner</param>
        /// <param name="queryBuilder">The queryBuilder</param>
        public BaseSqlDAO(string connString, QueryRunner queryRunner, IQueryBuilder queryBuilder)
        {
            if (string.IsNullOrEmpty(connString)) connString = DefaultConnString;
            this.ConnString = connString;
            this.queryRunner = queryRunner == null ? new QueryRunner() : queryRunner;
            this.queryBuilder = queryBuilder == null ? new QueryBuilder() : queryBuilder;
        }

        /// <summary>
        /// Accesor for the QueryRunner
        /// </summary>
        /// <returns>The QueryRunner</returns>
        public QueryRunner GetQueryRunner() {
            return this.queryRunner;
        }

        /// <summary>
        /// Mutator for the QueryRunner
        /// </summary>
        /// <param name="queryRunner">The QueryRunner</param>
        public void SetQueryRunner(QueryRunner queryRunner) {
            this.queryRunner = queryRunner;
        }

        /// <summary>
        /// Accesor for the QueryBuilder
        /// </summary>
        /// <returns>The QueryBuilder</returns>
        public IQueryBuilder GetQueryBuilder()
        {
            return this.queryBuilder;
        }

        /// <summary>
        /// Mutator for the QueryBuilder
        /// </summary>
        /// <param name="queryBuilder">The QueryBuilder</param>
        public void SetQueryBuilder(IQueryBuilder queryBuilder)
        {
            this.queryBuilder = queryBuilder;
        }

        /// <summary>
        /// Closes the specified SqlConnection,SqlCommand and SqlDataReader.
        /// <para>if one of the arguments is null no action is taken.</para>
        /// </summary>
        /// <param name="conn">an SqlConnection</param>
        /// <param name="cmd">an SqlCommand</param>
        /// <param name="rd"> an SqlDataReader</param>
        protected virtual void Close(DbConnection conn, DbCommand cmd, DbDataReader rd)
        {
            DbUtils.Close(conn, cmd, rd);
        }

        /// <summary>
        /// Returns an SqlConnection
        /// </summary>
        /// <returns>The database connection</returns>
        protected virtual DbConnection GetConnection()
        {
            DbConnection conn = null;
            try
            {
                conn = DatabaseFactory.CreateDatabase(this.ConnString).CreateConnection();
                conn.Open();
            }
            catch(Exception e) {
                LoggerHelper.Error("Unable to obtain a connection, Connection String [" + this.ConnString + "]", e);
                throw new Exception("Unable to obtain a connection", e);
            }

            return conn;
        }

        /// <summary>
        /// Returns the total number of records of the specified entity. 
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <exception cref="System.Exception">Thrown when a problem occur during the execution of the statement.</exception>
        /// <returns>The total number of records</returns>
        public virtual int GetTotalRecords(Entity entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }
            
            LoggerHelper.Info("Start");
            int total = 0;

            try
            {
                StringBuilder query = new StringBuilder();
                StringBuilder join = new StringBuilder();
                IDictionary<string, string> aliases = new Dictionary<string, string>();
                query.Append("SELECT COUNT(*) AS TOTAL ");
                this.GetQueryBuilder().BuildQueryFieldsSection(entity, query, join, aliases, true);
                query.Append(" FROM ").Append(this.GetQueryBuilder().GetTableName(entity)).Append(" M ");

                if (join.Length > 0)
                {
                    query.Append(join);
                }
                LoggerHelper.Debug(query.ToString());

                total = GetQueryRunner().ExecuteScalar(GetConnection(), query);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get " + entity.GetTableName() + " total records.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return total;
        }

        /// <summary>
        /// Returns the total number of records of the specified entity filtered by the specified filter. 
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <param name="filter">The filter information</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <exception cref="System.Exception">Thrown when a problem occur during the execution of the statement.</exception>
        /// <returns>The total number of filtered records</returns>
        public virtual int GetFilteredTotalRecords(Entity entity, FilterInfo filter)
        {
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

            LoggerHelper.Info("Start");
            int total = 0;

            try
            {
               StatementWrapper stmtWrapper = this.GetQueryBuilder().BuildFilteredTotalRecordsStatement(entity, filter);
               LoggerHelper.Debug(stmtWrapper.Query.ToString());

               total = GetQueryRunner().ExecuteScalar(GetConnection(), stmtWrapper);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get " + entity.GetTableName() + " filtered total records.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return total;
        }
    }
}
