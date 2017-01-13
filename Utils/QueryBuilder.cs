using System.Collections.Generic;
using System.Text;
using BS.Common.Entities;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using BS.Common.Dao;
using BS.Common.Utils;
using System.Configuration;

namespace BS.Common.Utils
{
    /// <summary>
    ///     Utility class that generates Microsodt SQL queries base on a Entity object.
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public class QueryBuilder : BaseQueryBuilder, IQueryBuilder
    {
        /// <summary>
        /// </summary>
        public static readonly string Param = "@p";
        private string Certificate = ConfigurationManager.AppSettings["Certificate"];
        private string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];

        /// <summary>
        /// Builds an SQL SELECT statement using the specified entity.
        /// <para>If there is fields with a ForeignKey specified the resulting statement will have the specified JOIN clause to the ForeignKey.TableName</para>
        /// <para>also the ForeignKey.JoinFields will be included in the SELECT if present.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The SELECT statement</returns>
        public StringBuilder BuildQuery(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            StringBuilder join = new StringBuilder();
            IDictionary<string, string> aliases = new Dictionary<string, string>();

            query.Append("SELECT ");
            BuildQueryFieldsSection(entity, query, join, aliases);
            query.Append(" FROM ").Append(GetTableName(entity)).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            WrapEncryptKey(entity,query);
            
            return query;
        }

        /// <summary>
        /// Builds an SQL SELECT statement using the specified entity and filter.
        /// <para>If there is fields with a ForeignKey specified the resulting statement will have the specified JOIN clause to the ForeignKey.TableName</para>
        /// <para>also the ForeignKey.JoinFields will be included in the SELECT if present.</para>
        /// <para>The FilterInfo is used to build the WHERE clause of the statement as well as to narrow the results to the specified length.</para>
        /// </summary>
        /// <param name="entity">An Entity, that contains all the table data to build the statement</param>
        /// <param name="filter">A FilterInfo, that contains the data to build the WHERE clause</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The SELECT statement</returns>
        public StringBuilder BuildQuery(Entity entity, FilterInfo filter)
        {
            StringBuilder query = new StringBuilder();
            StringBuilder join = new StringBuilder();
            IDictionary<string, string> aliases = new Dictionary<string, string>();

            query.Append("WITH FilteredTable AS ( ");
            query.Append("SELECT ");
            BuildQueryFieldsSection(entity, query, join, aliases);

            query.Append(",ROW_NUMBER() OVER (ORDER BY ");

            foreach (SortColumn sortColumn in filter.SortColumns)
            {
                ColumnInfo colInfo = filter.Columns[sortColumn.SortCol];
                query.Append(GetFieldName(entity, colInfo, aliases)).Append(" ").Append(sortColumn.SortDir).Append(",");
            }

            query.Remove(query.Length - 1, 1);
            query.Append(") AS RowNumber FROM ").Append(GetTableName(entity)).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            BuildQueryWhereSection(entity, filter, query, aliases);

            query.Append(" ) SELECT * FROM FilteredTable ");

            if (filter.Lenght > 0)
            {
                query.Append(" WHERE RowNumber BETWEEN ");
                query.Append(filter.Start + 1).Append(" AND ").Append(filter.Start + filter.Lenght);
            }
            
            WrapEncryptKey(entity, query);
            
            return query;
        }
        
        /// <summary>
        /// Creates an SQL SELECT statement using the specified entity and filter.
        /// The where section is created from the entity properties and the filter is used for the sort section.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="filter">The filter info</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The query</returns>
        /// <returns>The SELECT statement</returns>
        public StringBuilder BuildFindEntitiesQuery(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            StringBuilder join = new StringBuilder();
            IDictionary<string, string> aliases = new Dictionary<string, string>();

            query.Append("SELECT ");
            BuildQueryFieldsSection(entity, query, join, aliases);
            query.Append(" FROM ").Append(GetTableName(entity)).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            AppendWhere(query, searchType, entity, aliases, null);

            AppendSort(query, filter, entity, aliases);

            WrapEncryptKey(entity,query);
            
            return query;
        }

        /// <summary>
        /// Creates the filtered total query of the specified entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="filter">The filter</param>
        /// <returns>The query</returns>
        /// <returns>The SELECT statement</returns>
        public StringBuilder BuildFilteredTotalRecordsQuery(Entity entity, FilterInfo filter)
        {
            StringBuilder query = new StringBuilder();
            StringBuilder join = new StringBuilder();
            IDictionary<string, string> aliases = new Dictionary<string, string>();

            query.Append("SELECT COUNT(*) AS TOTAL ");
            BuildQueryFieldsSection(entity, query, join, aliases, true);
            query.Append(" FROM ").Append(GetTableName(entity)).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            BuildQueryWhereSection(entity, filter, query, aliases);

            return query;
        }

        /// <summary>
        /// Creates the where section of an sql SELECT statement from the specified entity and filter and appends the result to the specified query.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="filter">The join part of the query</param>
        /// <param name="query">The query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>        
        public void BuildQueryWhereSection(Entity entity, FilterInfo filter, StringBuilder query, IDictionary<string, string> aliases)
        {
            BuildQueryWhereSection(entity, filter, query, aliases, null);
        }

        /// <summary>
        /// Creates the select list of an sql SELECT statement from the specified entity and appends the result to the specified query.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="query">The query</param>
        /// <param name="join">The join part of the query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>
        public void BuildQueryFieldsSection(Entity entity, StringBuilder query, StringBuilder join, IDictionary<string, string> aliases)
        {
            BuildQueryFieldsSection(entity, query, join, aliases, false);
        }

        /// <summary>
        /// Creates the select list of an sql SELECT statement from the specified entity and appends the result to the specified query.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="query">The query</param>
        /// <param name="join">The join part of the query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>
        /// <param name="onlyJoin">Signals if the method will append the field section or not</param>
        public void BuildQueryFieldsSection(Entity entity, StringBuilder query, StringBuilder join, IDictionary<string, string> aliases, bool onlyJoin)
        {
            int j = 1;
            foreach (Field field in entity.GetFields())
            {
                if (!onlyJoin)
                {
                    if (field.Insertable || field.Updatable)
                    {
                        switch (field.DataType)
                        {
                            case Field.DBType.Date:
                                query.Append("CONVERT(VARCHAR, ").Append(GetFieldName(field, aliases)).Append(", 101) AS ").Append(GetFieldName(field)).Append(",");
                                break;                           
                            case Field.DBType.Encrypt:
                                AppendDecryptByKey(query, aliases, field);
                                break;
                            default:
                                query.Append(GetFieldName(field, aliases)).Append(",");
                                break;
                        }
                    }
                }

                if (field.ForeignKey != null)
                {
                    string joinAlias = "J" + j;
                    if (field.ForeignKey.Type == Field.FKType.Inner)
                    {
                        join.Append(" INNER ");
                    }
                    else if (field.ForeignKey.Type == Field.FKType.Left)
                    {
                        join.Append(" LEFT OUTER ");
                    }
                    else if (field.ForeignKey.Type == Field.FKType.Right)
                    {
                        join.Append(" RIGHT OUTER ");
                    }

                    join.Append(" JOIN [").Append(field.ForeignKey.TableName).Append("] ").Append(joinAlias);
                    join.Append(" ON ").Append(GetFieldName(field, aliases)).Append(" = ").Append(joinAlias).Append(".");
                    string joinField = Regex.Replace(field.ForeignKey.JoinField, @"J\d\.", joinAlias + ".");
                    join.Append(joinField).Append(" ");

                    foreach (string joinTableFieldName in field.ForeignKey.JoinFields)
                    {
                        if (!onlyJoin)
                        {
                            query.Append(joinAlias).Append(".").Append(joinTableFieldName).Append(",");
                        }
                        aliases.Add(joinTableFieldName, joinAlias);
                    }
                    j++;
                }
            }

            if (!onlyJoin)
            {
                query.Remove(query.Length - 1, 1);
            }
        }

        /// <summary>
        /// Builds an SQL INSERT statement using the specified entity.
        /// <para>Fields flag as not insertable will not be included in the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The INSERT statement</returns>
        public StringBuilder BuildInsertQuery(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            StringBuilder fieldValues = new StringBuilder();

            query.Append("INSERT INTO ").Append(GetTableName(entity)).Append("(");
            
            foreach (Field field in entity.GetFields())
            {
                if (!field.Id && field.Insertable)
                {
                    query.Append(GetFieldName(field)).Append(",");
                    fieldValues.Append(GetFieldValue(entity, field)).Append(",");
                }
            }
            
            query.Remove(query.Length - 1, 1);
            fieldValues.Remove(fieldValues.Length - 1, 1);
            
            query.Append(") VALUES(").Append(fieldValues).Append(") SELECT SCOPE_IDENTITY()");

            return query;
        }

        /// <summary>
        /// Builds an SQL UPDATE statement using the specified entity. 
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The UPDATE statement</returns>
        public StringBuilder BuildUpdateQuery(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();

            query.Append("UPDATE ").Append(GetTableName(entity)).Append(" SET ");
            foreach (Field field in entity.GetFields())
            {
                if (!field.Id && field.Updatable)
                {
                    query.Append(GetFieldName(field)).Append(" = ");
                    query.Append(GetFieldValue(entity, field)).Append(",");
                }
            }
            query.Remove(query.Length - 1, 1);
            query.Append(" WHERE ");
            query.Append(GetFieldName(entity.GetFieldId())).Append(" = ").Append(entity.GetEntityId());
            
            return query;
        }

        /// <summary>
        /// Builds an SQL UPDATE statement using the specified entity properties. 
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The UPDATE statement</returns>
        public StringBuilder BuildUpdateEntityQuery(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            query.Append("UPDATE ").Append(GetTableName(entity)).Append(" SET ");
            foreach (KeyValuePair<string, string> pair in entity.GetProperties())
            {
                Field field = entity.GetField(pair.Key);
                if (!field.Id)
                {
                    query.Append(GetFieldName(field)).Append(" = ");
                    query.Append(GetFieldValue(entity, field)).Append(",");
                }
            }

            query.Remove(query.Length - 1, 1);
            query.Append(" WHERE ");
            query.Append(GetFieldName(entity.GetFieldId())).Append(" = ").Append(entity.GetEntityId());

            return query;
        }

        /// <summary>
        /// Builds an SQL DELETE statement using the specified entity.
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The DELETE statement</returns>
        public StringBuilder BuildDeleteQuery(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            query.Append("DELETE FROM ").Append(GetTableName(entity)).Append(" WHERE ");
            query.Append(GetFieldName(entity.GetFieldId())).Append(" = ").Append(entity.GetEntityId());

            return query;
        }

        /// <summary>
        /// Builds an SQL INSERT statement using the specified instance.
        /// </summary>
        /// <param name="instance">An Object instance</param>
        /// <param name="idField">The id field</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The INSERT statement</returns>
        public StringBuilder BuildInsertQuery(Object instance, string idField)
        {
            return BuildInsertQuery(instance, idField, null);
        }

        /// <summary>
        /// Builds an SQL INSERT statement using the specified instance.
        /// </summary>
        /// <param name="instance">An Object instance</param>
        /// <param name="idField">The id field</param>
        /// <param name="defaultVals">Optional dictionary that contains default values for certain fields.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The INSERT statement</returns>
        public StringBuilder BuildInsertQuery(Object instance, string idField, IDictionary<string, string> defaultVals)
        {
            return BuildInsertQuery(instance, idField, defaultVals, true);
        }

        /// <summary>
        /// Builds an SQL INSERT statement using the specified instance.
        /// </summary>
        /// <param name="instance">An Object instance</param>
        /// <param name="idField">The id field</param>
        /// <param name="defaultVals">Optional dictionary that contains default values for certain fields.</param>
        /// <param name="outputId">Flag to indicate the inclusion (true) or exclusion (false) of the inserted id field</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The INSERT statement</returns>
        public StringBuilder BuildInsertQuery(Object instance, string idField, IDictionary<string, string> defaultVals, bool outputId)
        {
            CheckNulls(instance);

            StringBuilder query = new StringBuilder();
            StringBuilder fieldValues = new StringBuilder();

            Type type = instance.GetType();
            PropertyInfo[] props = type.GetProperties();

            query.Append("INSERT INTO ").Append(type.Name).Append("(");

            foreach (PropertyInfo prop in props)
            {
                if (defaultVals != null && defaultVals.ContainsKey(prop.Name) && !prop.Name.Equals(idField))
                {
                    query.Append(prop.Name).Append(",");
                    fieldValues.Append(defaultVals[prop.Name]).Append(",");
                }
                else if (prop.PropertyType == typeof(String) && prop.Name != idField)
                {
                    query.Append(prop.Name).Append(",");

                    string value = (string)prop.GetValue(instance, null);
                    if (null != value)
                        value = value.Replace("'", "''");

                    fieldValues.Append("'").Append(value).Append("',");
                }
                else if ((prop.PropertyType == typeof(int) || (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(decimal))) && prop.Name != idField)
                {
                    query.Append(prop.Name).Append(",");
                    fieldValues.Append(prop.GetValue(instance, null)).Append(",");
                }
            }

            query.Remove(query.Length - 1, 1);
            fieldValues.Remove(fieldValues.Length - 1, 1);

            if (outputId)
                query.Append(") OUTPUT INSERTED.").Append(idField);
            else
                query.Append(") ");

            query.Append(" VALUES(").Append(fieldValues).Append(")");

            return query;
        }

        /// <summary>
        /// Builds an SQL UPDATE statement using the specified instance. 
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="idField">The id field</param>
        /// <exception cref="System.ArgumentNullException">Thrown when instance is null</exception>
        /// <returns>The UPDATE statement</returns>
        public StringBuilder BuildUpdateQuery(Object instance, string idField)
        {
            return BuildUpdateQuery(instance, idField, null);
        }

        /// <summary>
        /// Builds an SQL UPDATE statement using the specified instance. 
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="idField">The id field</param>
        /// <param name="defaultVals">Optional dictionary that contains default values for certain fields.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when instance is null</exception>
        /// <returns>The UPDATE statement</returns>
        public StringBuilder BuildUpdateQuery(Object instance, string idField, IDictionary<string, string> defaultVals)
        {
            CheckNulls(instance);

            Type type = instance.GetType();
            StringBuilder query = new StringBuilder();
            PropertyInfo[] props = type.GetProperties();
            string idValue = "";

            query.Append("UPDATE ").Append(type.Name).Append(" SET ");
            foreach (PropertyInfo prop in props)
            {
                if (defaultVals != null && defaultVals.ContainsKey(prop.Name) && !prop.Name.Equals(idField))
                {
                    query.Append(prop.Name).Append(" = ").Append(defaultVals[prop.Name]).Append(" ,"); ;
                }
                else if (prop.PropertyType == typeof(String))
                {
                    if (prop.Name == idField)
                    {
                        idValue = (string)prop.GetValue(instance, null);
                    }
                    else
                    {
                        query.Append(prop.Name).Append(" = '");
                        query.Append(prop.GetValue(instance, null)).Append("' ,");
                    }
                }
                else if (prop.PropertyType == typeof(int) || (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(decimal)))
                {
                    if (prop.Name == idField)
                    {
                        idValue = prop.GetValue(instance, null).ToString();
                    }
                    else
                    {
                        query.Append(prop.Name).Append(" = ");
                        query.Append(prop.GetValue(instance, null)).Append(" ,");
                    }
                }
            }

            query.Remove(query.Length - 1, 1);
            query.Append(" WHERE ");
            query.Append(idField).Append(" = ").Append(idValue);

            return query;
        }

        /// <summary>
        /// Builds a DELETE statement using the specified instance.
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="idField">The id field</param>
        /// <returns>The DELETE statement</returns>
        public StringBuilder BuildDeleteQuery(Object instance, string idField)
        {
            CheckNulls(instance);

            StringBuilder query = new StringBuilder();
            Type type = instance.GetType();
            PropertyInfo[] props = type.GetProperties();
            string idValue = "";

            foreach (PropertyInfo prop in props)
            {
                if (prop.PropertyType == typeof(String) && prop.Name == idField)
                {
                    idValue = (string)prop.GetValue(instance, null);
                    break;
                }
            }


            query.Append("DELETE FROM ").Append(type.Name).Append(" WHERE ");
            query.Append(idField).Append(" = ").Append(idValue);
            return query;
        }

        /// <summary>
        /// Builds an INSERT statement using the specified entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The Statement Wrapper containing the INSERT statement and the statement parameters.</returns>

        public StatementWrapper BuildInsertStatement(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();
            
            BuildInsertStatement(entity, query, queryParams);

            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Builds an INSERT statement using the specified entity and append it to the provided StringBuilder 
        /// as well as the statement parameters to the specified list.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="query">the stringbuilder</param>
        /// <param name="queryParams">the statement param list</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        public void BuildInsertStatement(Entity entity, StringBuilder query, IList<DBParam> queryParams)
        {
            BuildInsertStatement(entity, query, queryParams, null);
        }

        /// <summary>
        /// Builds an INSERT statement using the specified entity and append it to the provided StringBuilder 
        /// as well as the statement parameters to the specified list.
        /// 
        /// If a field value is found in the defaultVals list it will be used instead of the entity value.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="query">the stringbuilder</param>
        /// <param name="queryParams">the statement param list</param>
        /// <param name="defaultVals">the defaultVals list</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        public void BuildInsertStatement(Entity entity, StringBuilder query, IList<DBParam> queryParams, IDictionary<string, string> defaultVals)
        {            
            StringBuilder fieldValues = new StringBuilder();

            query.Append("INSERT INTO ").Append(GetTableName(entity)).Append("(");

            foreach (Field field in entity.GetFields())
            {
                if (!field.Id && field.Insertable)
                {
                    query.Append(GetFieldName(field)).Append(",");
                    if ((BS.Common.Entities.Field.DBType.Date == field.DataType || BS.Common.Entities.Field.DBType.DateTime == field.DataType) && entity.GetProperty(field.Name).ToUpper().Contains("GETDATE"))
                    {
                        fieldValues.Append("GETDATE(),");
                    }
                    else if (defaultVals != null && defaultVals.ContainsKey(field.Name))
                    {
                        fieldValues.Append(defaultVals[field.Name]).Append(",");
                    }
                    else if (BS.Common.Entities.Field.DBType.Encrypt == field.DataType)
                    {
                        AppendEncryptByKey(entity, fieldValues, field);
                    }
                    else
                    {
                        fieldValues.Append(Param).Append(queryParams.Count).Append(",");
                        queryParams.Add(new DBParam(queryParams, GetStmtFieldValue(entity, field), field.GetDbType()));
                    }
                }
            }

            query.Remove(query.Length - 1, 1);
            fieldValues.Remove(fieldValues.Length - 1, 1);

            query.Append(") ");
            query.Append(" VALUES(").Append(fieldValues).Append(") ");

            query.Append("SELECT SCOPE_IDENTITY()");
            WrapEncryptKey(entity, query);
            
        }

        /// <summary>
        /// Builds an UPDATE statement using the specified entity. 
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when instance is null</exception>
        /// <returns>The Statement Wrapper containing the UPDATE statement and the statement parameters.</returns>
        public StatementWrapper BuildUpdateStatement(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();

            BuildUpdateStatement(entity, query, queryParams);

            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Builds an UPDATE statement using the specified entity and append it to the provided StringBuilder 
        /// as well as the statement parameters to the specified list.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="query">the stringbuilder</param>
        /// <param name="queryParams">the statement param list</param>
        /// <exception cref="System.ArgumentNullException">Thrown when instance is null</exception>
        /// <returns>The Statement Wrapper containing the UPDATE statement and the statement parameters.</returns>
        public void BuildUpdateStatement(Entity entity, StringBuilder query, IList<DBParam> queryParams)
        {
            query.Append("UPDATE ").Append(GetTableName(entity)).Append(" SET ");
            foreach (Field field in entity.GetFields())
            {
                if (!field.Id && field.Updatable)
                {
                    query.Append(GetFieldName(field)).Append(" = ");
                    if ((BS.Common.Entities.Field.DBType.Date == field.DataType || BS.Common.Entities.Field.DBType.DateTime == field.DataType) && entity.GetProperty(field.Name).ToUpper().Contains("GETDATE"))
                    {
                        query.Append("GETDATE(),");
                    }
                    else if (BS.Common.Entities.Field.DBType.Encrypt == field.DataType)
                    {
                        AppendEncryptByKey(entity, query, field);
                    }
                    else
                    {
                        query.Append(Param).Append(queryParams.Count).Append(",");
                        queryParams.Add(new DBParam(queryParams, GetStmtFieldValue(entity, field), field.GetDbType()));
                    }
                }
            }
            query.Remove(query.Length - 1, 1);
            query.Append(" WHERE ");
            query.Append(GetFieldName(entity.GetFieldId())).Append(" = ").Append(Param + queryParams.Count);
            queryParams.Add(new DBParam(queryParams, entity.GetEntityId(), entity.GetFieldId().GetDbType()));

            WrapEncryptKey(entity,query);            
        }

        /// <summary>
        /// Builds an UPDATE statement using the specified entity properties. 
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The Statement Wrapper containing the UPDATE statement and the statement parameters.</returns>
        public StatementWrapper BuildUpdateEntityStatement(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();

            BuildUpdateEntityStatement(entity, query, queryParams);
            
            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Builds an UPDATE statement using the specified entity properties. 
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// 
        /// The statement will be append it to the provided StringBuilder 
        /// as well as the statement parameters to the specified list.
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <param name="query">the stringbuilder</param>
        /// <param name="queryParams">the statement param list</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The Statement Wrapper containing the UPDATE statement and the statement parameters.</returns>
        public void BuildUpdateEntityStatement(Entity entity, StringBuilder query, IList<DBParam> queryParams)
        {
            query.Append("UPDATE ").Append(GetTableName(entity)).Append(" SET ");
            foreach (KeyValuePair<string, string> pair in entity.GetProperties())
            {
                Field field = entity.GetField(pair.Key);
                if (!field.Id)
                {
                    query.Append(GetFieldName(field)).Append(" = ");
                    if ((BS.Common.Entities.Field.DBType.Date == field.DataType || BS.Common.Entities.Field.DBType.DateTime == field.DataType) && entity.GetProperty(field.Name).ToUpper().Contains("GETDATE"))
                    {
                        query.Append("GETDATE(),");
                    }
                    else
                    {
                        query.Append(Param).Append(queryParams.Count).Append(",");
                        queryParams.Add(new DBParam(queryParams, GetStmtFieldValue(entity, field), field.GetDbType()));
                    }
                }
            }

            query.Remove(query.Length - 1, 1);
            query.Append(" WHERE ");
            query.Append(GetFieldName(entity.GetFieldId())).Append(" = ").Append(Param + queryParams.Count);
            queryParams.Add(new DBParam(queryParams, entity.GetEntityId(), entity.GetFieldId().GetDbType()));
        }

        /// <summary>
        /// Builds an UPDATE statement using the specified entity properties. 
        /// <para>The specified whereEntity is used to build the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <param name="whereEntity">the where entity</param>
        /// <returns>The Statement Wrapper containing the UPDATE statement and the statement parameters.</returns>
        public StatementWrapper BuildUpdateEntityStatement(Entity entity, Entity whereEntity)
        {
            CheckNulls(entity);
            CheckNulls(whereEntity);

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();

            BuildUpdateEntityStatement(entity, whereEntity, query, queryParams);

            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Builds an UPDATE statement using the specified entity properties and append it to the specified query. 
        /// <para>The specified whereEntity is used to build the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <param name="whereEntity">the where entity</param>
        /// <param name="query">the stringbuilder</param>
        /// <param name="queryParams">the statement param list</param>
        public void BuildUpdateEntityStatement(Entity entity, Entity whereEntity, StringBuilder query, IList<DBParam> queryParams)
        {
            query.Append("UPDATE ").Append(GetTableName(entity)).Append(" SET ");
            foreach (KeyValuePair<string, string> pair in entity.GetProperties())
            {
                Field field = entity.GetField(pair.Key);
                if (!field.Id)
                {
                    query.Append(GetFieldName(field)).Append(" = ");
                    if ((BS.Common.Entities.Field.DBType.Date == field.DataType || BS.Common.Entities.Field.DBType.DateTime == field.DataType) && entity.GetProperty(field.Name).ToUpper().Contains("GETDATE"))
                    {
                        query.Append("GETDATE(),");
                    }
                    else
                    {
                        query.Append(Param).Append(queryParams.Count).Append(",");
                        queryParams.Add(new DBParam(queryParams, GetStmtFieldValue(entity, field), field.GetDbType()));
                    }
                }
            }

            query.Remove(query.Length - 1, 1);
            query.Append(" WHERE ");

            foreach (KeyValuePair<string, string> pair in whereEntity.GetProperties())
            {
                Field field = whereEntity.GetField(pair.Key);
                if (!field.Id)
                {
                    query.Append(GetFieldName(field)).Append(" = ");
                    if ((BS.Common.Entities.Field.DBType.Date == field.DataType || BS.Common.Entities.Field.DBType.DateTime == field.DataType) && whereEntity.GetProperty(field.Name).ToUpper().Contains("GETDATE"))
                    {
                        query.Append("GETDATE(),");
                    }
                    else
                    {
                        query.Append(Param).Append(queryParams.Count).Append(",");
                        queryParams.Add(new DBParam(queryParams, GetStmtFieldValue(whereEntity, field), field.GetDbType()));
                    }
                }
            }

            query.Remove(query.Length - 1, 1);
        }

        /// <summary>
        /// Builds a DELETE statement using the specified entity.
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <returns>The Statement Wrapper containing the DELETE statement and the statement parameters.</returns>
        public StatementWrapper BuildDeleteStatement(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();

            BuildDeleteStatement(entity, query, queryParams);

            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Builds a DELETE statement using the specified entity.
        /// 
        /// The statement will be append it to the provided StringBuilder 
        /// as well as the statement parameters to the specified list.
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <param name="query">the StringBuilder</param>
        /// <param name="queryParams">the statement param list</param>
        public void BuildDeleteStatement(Entity entity, StringBuilder query, IList<DBParam> queryParams)
        {
            query.Append("DELETE FROM ").Append(GetTableName(entity)).Append(" WHERE ");
            query.Append(GetFieldName(entity.GetFieldId())).Append(" = ").Append(Param + queryParams.Count);
            queryParams.Add(new DBParam(queryParams, entity.GetEntityId(), entity.GetFieldId().GetDbType()));
        }

        /// <summary>
        /// Builds a DELETE statement using the specified entity properties to construct the WHERE clause.
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <returns>The Statement Wrapper containing the DELETE statement and the statement parameters.</returns>
        public StatementWrapper BuildDeleteEntitiesStatement(Entity entity)
        {
            CheckNulls(entity);

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();

            BuildDeleteEntitiesStatement(entity, query, queryParams);

            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Builds a DELETE statement using the specified entity properties to construct the WHERE clause.
        /// 
        /// The statement will be append it to the provided StringBuilder 
        /// as well as the statement parameters to the specified list.
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <param name="query">the StringBuilder</param>
        /// <param name="queryParams">the statement param list</param>
        public void BuildDeleteEntitiesStatement(Entity entity, StringBuilder query, IList<DBParam> queryParams)
        {
            query.Append("DELETE FROM ").Append(GetTableName(entity));

            StringBuilder where = new StringBuilder();

            foreach (KeyValuePair<string, string> pair in entity.GetProperties())
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    where.Append("[").Append(pair.Key).Append("]").Append(" = ").Append(QueryBuilder.Param).Append(queryParams.Count).Append(" AND ");
                    queryParams.Add(new DBParam(queryParams, pair.Value, DbType.String, false));
                }
            }

            if (where.Length > 0)
            {
                where.Remove(where.Length - 4, 4);
                query.Append(" WHERE ").Append(where);
            }
        }

        /// <summary>
        /// Creates the filtered total query of the specified entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="filter">The filter</param>
        /// <returns>The Statement Wrapper containing the SELECT statement and the statement parameters.</returns>
        public StatementWrapper BuildFilteredTotalRecordsStatement(Entity entity, FilterInfo filter)
        {
            StringBuilder query = new StringBuilder();
            StringBuilder join = new StringBuilder();
            IDictionary<string, string> aliases = new Dictionary<string, string>();

            query.Append("SELECT COUNT(*) AS TOTAL ");
            BuildQueryFieldsSection(entity, query, join, aliases, true);
            query.Append(" FROM ").Append(GetTableName(entity)).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            IList<DBParam> queryParams = new List<DBParam>();
            BuildQueryWhereSection(entity, filter, query, aliases, queryParams);

            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Builds a SELECT statement using the specified entity and filter.
        /// <para>If there is fields with a ForeignKey specified the resulting statement will have the specified JOIN clause to the ForeignKey.TableName</para>
        /// <para>also the ForeignKey.JoinFields will be included in the SELECT if present.</para>
        /// <para>The FilterInfo is used to build the WHERE clause of the statement as well as to narrow the results to the specified length.</para>
        /// </summary>
        /// <param name="entity">An Entity, that contains all the table data to build the statement</param>
        /// <param name="filter">A FilterInfo, that contains the data to build the WHERE clause</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The Statement Wrapper containing the SELECT statement and the statement parameters.</returns>

        public StatementWrapper BuildSelectStatement(Entity entity, FilterInfo filter)
        {
            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();
            StringBuilder join = new StringBuilder();
            IDictionary<string, string> aliases = new Dictionary<string, string>();

            query.Append("WITH FilteredTable AS ( ");
            query.Append("SELECT ");
            BuildQueryFieldsSection(entity, query, join, aliases);

            query.Append(",ROW_NUMBER() OVER (ORDER BY ");

            foreach (SortColumn sortColumn in filter.SortColumns)
            {
                ColumnInfo colInfo = filter.Columns[sortColumn.SortCol];
                query.Append(GetFieldName(entity, colInfo, aliases)).Append(" ").Append(sortColumn.SortDir).Append(",");
            }

            query.Remove(query.Length - 1, 1);
            query.Append(") AS RowNumber FROM ").Append(GetTableName(entity)).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            BuildQueryWhereSection(entity, filter, query, aliases, queryParams);

            query.Append(" ) SELECT * FROM FilteredTable ");

            if (filter.Lenght > 0)
            {
                query.Append(" WHERE RowNumber BETWEEN ");
                query.Append(filter.Start + 1).Append(" AND ").Append(filter.Start + filter.Lenght);
            }
            
            WrapEncryptKey(entity,query);
            
            return new StatementWrapper(query, queryParams); 
        }

        /// <summary>
        /// Creates a SELECT statement using the specified entity and filter.
        /// The where section is created from the entity properties and the filter is used for the sort section.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="filter">The filter info</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The query</returns>
        /// <returns>The Statement Wrapper containing the SELECT statement and the statement parameters.</returns>
        public StatementWrapper BuildFindEntitiesStatement(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType)
        {
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }
            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();
            StringBuilder join = new StringBuilder();
            IDictionary<string, string> aliases = new Dictionary<string, string>();

            query.Append("SELECT ");
            BuildQueryFieldsSection(entity, query, join, aliases);
            query.Append(" FROM ").Append(GetTableName(entity)).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            AppendWhere(query, searchType, entity, aliases, queryParams);
            
            AppendSort(query, filter, entity, aliases);
           
            WrapEncryptKey(entity,query);
            
            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Builds a SELECT statement with the specified Aggregated Functions in the aggregatedInfo object.
        /// 
        /// And sets the fields of the specified Aggregated Entity
        /// </summary>
        /// <param name="entity">an Entity</param>
        /// <param name="aggregateInfo">The aggregatedinfo object</param>
        /// <param name="aggregateEntity">The aggregated entity</param>
        /// <param name="filter">The filter info</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The Statement Wrapper containing the SELECT statement and the statement parameters.</returns>
        public StatementWrapper BuildAggregateStatement(Entity entity, AggregateInfo aggregateInfo, Entity aggregateEntity, FilterInfo.SearchType searchType, FilterInfo filter)
        {
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }
            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();
            StringBuilder join = new StringBuilder();
            StringBuilder having = new StringBuilder();
            IDictionary<string, string> aliases = new Dictionary<string, string>();

            query.Append("SELECT ");

            BuildQueryFieldsSection(entity, query, join, aliases, true);
            
            //Appending group by fields
            StringBuilder fieldNames = new StringBuilder();
            foreach (string field in aggregateInfo.GetGroupByFields())
            {
                Field _field = entity.GetField(field);
                string fieldName = GetFieldName(_field, aliases);
                fieldNames.Append(fieldName).Append(", ");
                query.Append(fieldName).Append(" AS ").Append(_field.DBName).Append(", ");
                aggregateEntity.SetField(_field);
            }

            //Appendding aggregate functions
            string postFix = "";
            for(int i = 0; i < aggregateInfo.Functions.Count; i++)
            {
                AggregateFunc func = aggregateInfo.Functions[i];
                if (i > 0 && func.Alias.Equals("Aggregate")) postFix = i.ToString();

                string fieldName = func.FieldName.Equals("*") ? func.FieldName : GetFieldName(entity.GetField(func.FieldName), aliases);

                query.Append(func.Function.ToUpper()).Append("(").Append(fieldName).Append(") AS ").Append(func.Alias + postFix).Append(", ");
                aggregateEntity.SetField(new Field(func.Alias + postFix));

                if (!string.IsNullOrEmpty(func.HavingOperator) && !string.IsNullOrEmpty(func.HavingValue))
                {
                    having.Append(func.Function.ToUpper()).Append("(").Append(GetFieldName(entity.GetField(func.FieldName), aliases)).Append(") ").Append(func.HavingOperator).Append(" ").Append(func.HavingValue).Append(" AND ");
                }                
            }

            if (aggregateInfo.Functions.Count > 0) query.Remove(query.Length - 2, 2);

            //Appending grouping
            if (!string.IsNullOrEmpty(aggregateInfo.GroupingField))
            {
                Field _field = entity.GetField(aggregateInfo.GroupingField);
                string fieldName = GetFieldName(_field, aliases);

                query.Append(", GROUPING(").Append(fieldName).Append(") AS Grouping ");
            }

            query.Append(" FROM ").Append(GetTableName(entity)).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            if (filter == null)
            {
                AppendWhere(query, searchType, entity, aliases, queryParams);
            } else {
                BuildQueryWhereSection(entity, filter, query, aliases);
            }
                       
            if (fieldNames.Length > 0) query.Append(" GROUP BY ").Append(fieldNames.Remove(fieldNames.Length - 2, 2));
            
            if (!string.IsNullOrEmpty(aggregateInfo.GroupingField))
            {
                query.Append(" WITH ROLLUP ");
                aggregateEntity.SetField(new Field("Grouping"));
            }

            if (having.Length > 0)
            {
                having.Remove(having.Length - 5, 5);
                query.Append(" HAVING ").Append(having);
            }

            return new StatementWrapper(query, queryParams);
        }

        /// <summary>
        /// Creates the where section of a SELECT statement from the specified entity and filter and appends the result to the specified query also adds the
        /// statement parameters to the specified queryParams list.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="filter">The join part of the query</param>
        /// <param name="query">The query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>
        /// <param name="queryParams">The query params list</param>
        public void BuildQueryWhereSection(Entity entity, FilterInfo filter, StringBuilder query, IDictionary<string, string> aliases, IList<DBParam> queryParams)
        {
            StringBuilder where = new StringBuilder();
            StringBuilder andWhere = new StringBuilder();
            StringBuilder orWhere = new StringBuilder();

            List<ColumnInfo> searchableCols = ((List<ColumnInfo>)filter.Columns).FindAll(c => c.Searchable); //Searchable columns
            foreach (ColumnInfo col in searchableCols)
            {
                string fieldName = GetFieldName(entity, col, aliases);
                Field field = entity.GetField(col.Name);

                if (!"".Equals(filter.Search))
                {
                    if (field != null)
                    {
                        fieldName = GetFieldName(field, aliases);
                        if (field.DataType == Field.DBType.Date)
                        {
                            fieldName = " CONVERT(VARCHAR," + fieldName + ", 101) ";
                        }
                        else if (field.DataType == Field.DBType.Encrypt)
                        {
                            fieldName = " CONVERT(VARCHAR,, DecryptByKey(" + fieldName + ")) "; 
                        }
                    }

                    AppendLikeField(orWhere, fieldName, filter.Search, "OR", queryParams);
                }

                if (!string.IsNullOrEmpty(col.Search)) {
                    string value = col.Search;
                    // the following logic is to support backward compatibility before calling AppendSearchCondition
                    if (col.SearchType == FilterInfo.ColumnSearchType.LIKE && value.IndexOf("_RANGE_") == -1 && !value.StartsWith("LIST_")) value = "LIKE_" + value;
                    else if (col.SearchType == FilterInfo.ColumnSearchType.NULL)
                    {
                        value = value == "NULL" ? "NULL" : "NOT_NULL";
                    }
                    else if (field.DataType == Field.DBType.Encrypt)
                    {
                        fieldName = " CONVERT(VARCHAR, DecryptByKey(" + fieldName + ")) ";
                    }
                    //TODO: validate if the field can be obtained inside AppendSearchCondition
                    AppendSearchCondition(andWhere, field, fieldName, value, "AND", aliases, queryParams);
                }                
            }

            if (orWhere.Length > 0)
            {
                orWhere.Remove(orWhere.Length - 2, 2);
                where.Append("(").Append(orWhere).Append(") ");
            }

            if (andWhere.Length > 0)
            {
                TrimStringBuilder(andWhere);
                andWhere.Remove(andWhere.Length - 3, 3);
                if (orWhere.Length > 0)
                {
                    where.Append(" AND ");
                }
                where.Append(andWhere);
            }

            if (where.Length > 0)
            {
                query.Append(" WHERE ").Append(where);
            }
        }

        private void TrimStringBuilder(StringBuilder b)
        {
            do
            {
                if(char.IsWhiteSpace(b[b.Length - 1]))
                {
                     b.Remove(b.Length - 1,1);
                }
            }
            while(char.IsWhiteSpace(b[b.Length - 1]));
        }

        /// <summary>
        /// Constructs a search condition from the specified field and append it to the specified WHERE clause.
        /// </summary>
        /// <param name="where">the WHERE clase</param>
        /// <param name="field">The field</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="value">the value of the field</param>
        /// <param name="type">logical operator AND or OR</param>
        /// <param name="aliases">the list of table aliases</param>
        /// <param name="queryParams">the list of the statement params</param>
        public void AppendSearchCondition(StringBuilder where, Field field, string fieldName, string value, string type, IDictionary<string, string> aliases, IList<DBParam> queryParams)
        {
            if (field != null)
            {
                if (field.DataType != Field.DBType.Encrypt)
                {
                    fieldName = GetFieldName(field, aliases);
                }
            }

            if (value.Equals("NOT_NULL")) //must be capital letters
            {
                where.Append(" ").Append(fieldName).Append(" ").Append("IS NOT NULL").Append(" ").Append(type).Append(" ");
            }
            else if (value == "NULL")//must be capital letters
            {
                where.Append(" ").Append(fieldName).Append(" ").Append("IS NULL").Append(" ").Append(type).Append(" ");
            }
            else {
                if (value.StartsWith("NOT_"))
                {
                    where.Append(" NOT ");
                    value = value.Substring("NOT_".Length);
                }
                
                if (value.IndexOf("_RANGE_", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    AppendRange(where, fieldName, value, queryParams, field, type);
                }
                else if (value.StartsWith("LIST_", StringComparison.InvariantCultureIgnoreCase))
                {
                    AppendList(where, fieldName, value, type, queryParams);
                }            
                else if (value.StartsWith("LIKE_"))//must be capital letters
                {
                    AppendLikeField(where, fieldName, value.Substring("LIKE_".Length), type, queryParams);
                }
                else
                {
                    AppendField(where, fieldName, value, type, queryParams, field);
                }
            }
        }

        /// <summary>
        /// Constructs a WHERE clause from the specified entity and append it to the specified statement.
        /// </summary>
        /// <param name="query">the statement</param>
        /// <param name="searchType">logical operator AND or OR</param>
        /// <param name="entity">the entity</param>
        /// <param name="aliases">the list of table aliases</param>
        /// <param name="queryParams">the list of the statement params</param>
        public void AppendWhere(StringBuilder query, FilterInfo.SearchType searchType, Entity entity, IDictionary<string, string> aliases, IList<DBParam> queryParams)
        {
            StringBuilder where = new StringBuilder();

            string type = "OR";
            if (searchType == FilterInfo.SearchType.AND)
            {
                type = "AND";
            }

            foreach (KeyValuePair<string, string> pair in entity.GetProperties())
            {                
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    Field field = entity.GetField(pair.Key); //TODO: validate if the field can be obtained inside AppendSearchCondition
                    AppendSearchCondition(where, field, pair.Key, pair.Value, type, aliases, queryParams);
                }
            }

            if (where.Length > 0)
            {
                where.Remove(where.Length - (type.Length + 1), (type.Length + 1));
                query.Append(" WHERE ").Append(where);
            }
        }

        /// <summary>
        /// Creates a LIKE type predicate using the specified fieldname and value and append it to the specified WHERE clause.
        /// <para>
        /// If the queryParams is not null it will create a parameterized predicate 
        /// </para>
        /// </summary>
        /// <param name="where">the WHERE clause</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="value">the value of the field</param>
        /// <param name="type">logical operator AND or OR</param>
        /// <param name="queryParams">the list of the statement params</param>
        public void AppendLikeField(StringBuilder where, string fieldName, string value, string type, IList<DBParam> queryParams)
        {
            where.Append(" ").Append(fieldName).Append(" LIKE ");
            if (queryParams == null)
            {
                where.Append("'%").Append(value.Replace("'", "''")).Append("%' ").Append(type);
            }
            else
            {
                where.Append(Param + queryParams.Count).Append(" ").Append(type);
                queryParams.Add(new DBParam(queryParams, value, true));
            }
        }

        /// <summary>
        /// Creates an EQUALS type predicate using the specified fieldname and value and append it to the specified WHERE clause.
        /// <para>
        /// If the queryParams is not null it will create a parameterized predicate 
        /// </para>
        /// </summary>
        /// <param name="where">the WHERE clause</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="value">the value of the field</param>
        /// <param name="type">logical operator AND or OR</param>
        /// <param name="queryParams">the list of the statement params</param>
        /// <param name="field">the field</param>
        public void AppendField(StringBuilder where, string fieldName, string value, string type, IList<DBParam> queryParams, Field field)
        {
            where.Append(" ").Append(fieldName).Append(" = ");
            if (queryParams == null)
            {
                where.Append("'").Append(value.Replace("'", "''")).Append("' ");
            }
            else
            {
                where.Append(Param + queryParams.Count).Append(" ");
                queryParams.Add(new DBParam(queryParams, value, field == null ? DbType.String : field.GetDbType()));
            }
            
            where.Append(type);
        }

        /// <summary>
        /// Creates an EQUALS type predicate using the specified fieldname and value and append it to the specified WHERE clause.
        /// </summary>
        /// <param name="where">the WHERE clause</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="value">the value of the field</param>
        /// <param name="type">logical operator AND or OR</param>
        public void AppendField(StringBuilder where, string fieldName, string value, string type)
        {
            AppendField(where, fieldName, value, type, null, null);
        }

        /// <summary>
        /// Creates a RANGE type predicate using the specified fieldname and value and append it to the specified WHERE clause.
        /// <para>
        /// If the queryParams is not null it will create a parameterized predicate 
        /// </para>
        /// </summary>
        /// <param name="query">the WHERE clause</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="value">the value of the field</param>
        /// <param name="queryParams">the list of the statement params</param>
        /// <param name="field">the field</param>
        /// <param name="type">logical operator AND or OR</param>
        public void AppendRange(StringBuilder query, string fieldName, string value, IList<DBParam> queryParams, Field field,string type)
        {
            string[] rangeValues = splitIgnoreCase(value, "_RANGE_");
            query.Append(" (");

            if (!string.IsNullOrEmpty(rangeValues[0]))
            {
                query.Append(fieldName).Append(" >= ");
                if(queryParams == null) {
                    query.Append("'").Append(rangeValues[0]).Append("'");
                } else {
                    query.Append(Param + queryParams.Count).Append("");
                    queryParams.Add(new DBParam(queryParams, rangeValues[0], field.GetDbType()));
                }
            }
            if (!string.IsNullOrEmpty(rangeValues[0]) && !string.IsNullOrEmpty(rangeValues[1])) query.Append(" AND ");

            if (!string.IsNullOrEmpty(rangeValues[1]))
            {
                query.Append(fieldName).Append(" <= ");
                if (queryParams == null)
                {
                    query.Append("'").Append(rangeValues[1]).Append("'"); ;
                }
                else
                {
                    query.Append(Param + queryParams.Count).Append("");
                    queryParams.Add(new DBParam(queryParams, rangeValues[1], field.GetDbType()));
                }
            }
            query.Append(") ").Append(type).Append(" ");
        }

        /// <summary>
        /// Creates a RANGE type predicate using the specified fieldname and value and append it to the specified WHERE clause.
        /// </summary>
        /// <param name="query">the WHERE clause</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="value">the value of the field</param>
        /// <param name="type">logical operator AND or OR</param>
        public void AppendRange(StringBuilder query, string fieldName, string value, string type)
        {
            AppendRange(query, fieldName, value, null, null, type);
        }

        /// <summary>
        /// Creates an IN type predicate using the specified fieldname and value and append it to the specified WHERE clause.
        /// <para>
        /// If the queryParams is not null it will create a parameterized predicate 
        /// </para>
        /// </summary>
        /// <param name="where">the WHERE clause</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="value">the value of the field</param>
        /// <param name="type">logical operator AND or OR</param>
        /// <param name="queryParams">the list of the statement params</param>
        public void AppendList(StringBuilder where, string fieldName, string value, string type, IList<DBParam> queryParams)
        {
            where.Append(" ");

            value = replaceStringAtIndex(value, "LIST_").Replace("LIST_", "");
            if (queryParams == null)
            {
                where.Append(fieldName).Append(" IN (").Append(value).Append(") ").Append(type).Append(" ");
                return;
            }
            
            where.Append(" ").Append(fieldName).Append(" IN (");
            string[] list = value.Split(new string[] { "," }, StringSplitOptions.None);
            foreach (string val in list)
            {
                where.Append(Param + queryParams.Count).Append(",");
                queryParams.Add(new DBParam(queryParams, val, DbType.String, false));
            }

            if (list.Length > 0) where.Remove(where.Length - 1, 1);

            where.Append(") ").Append(type).Append(" ");
        }

        /// <summary>
        /// Creates an IN type predicate using the specified fieldname and value and append it to the specified WHERE clause.
        /// </summary>
        /// <param name="where">the WHERE clause</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="value">the value of the field</param>
        /// <param name="type">logical operator AND or OR</param>
        public void AppendList(StringBuilder where, string fieldName, string value, string type)
        {
            AppendList(where, fieldName, value, type, null);            
        }

        /// <summary>
        /// Creates the ORDER BY clause and append it to the specified statement.
        /// </summary>
        /// <param name="query">the statement</param>
        /// <param name="filter">the filter info instance</param>
        /// <param name="entity">the entity</param>
        /// <param name="aliases">the list of table aliases</param>
        public void AppendSort(StringBuilder query, FilterInfo filter, Entity entity, IDictionary<string, string> aliases)
        {
            if (filter != null && filter.SortColumns.Count > 0)
            {
                query.Append(" ORDER BY ");
                foreach (SortColumn sortCol in filter.SortColumns)
                {
                    Field field = entity.GetField(filter.Columns[sortCol.SortCol].Name);
                    query.Append(GetFieldName(field, aliases)).Append(" ").Append(sortCol.SortDir).Append(",");
                }
                query.Remove(query.Length - 1, 1);
            }
        }

        /// <summary>
        /// Determines the database field name that will be used in the SQL statement for the specified field
        /// </summary>
        /// <param name="field">The Field</param>
        /// <returns>The database field name</returns>
        public string GetFieldName(Field field)
        {
            return GetFieldName(field, null);
        }

        /// <summary>
        /// Determines the database field name that will be used in the SQL statement for the specified field
        /// </summary>
        /// <param name="field">The Field</param>
        /// <param name="aliases">A dictionary of database aliases</param>
        /// <returns>The database field name</returns>
        public string GetFieldName(Field field, IDictionary<string, string> aliases)
        {
            string dbName = string.IsNullOrEmpty(field.DBName) ? field.Name : field.DBName; //this should not happen because DBName is "not null" in the DB
            string prefix = aliases != null ? "M." : ""; //when there is no aliases prexis will be empty
            if (aliases != null && aliases.ContainsKey(field.Name))
            {
                return aliases[field.Name] + ".[" + dbName + "]";
            }
            else if (aliases != null)
            {
                //search to see if there is a " AS " in one of the aliases keys
                foreach (KeyValuePair<string, string> pair in aliases)
                {
                    if (pair.Key != null && pair.Key.IndexOf(" AS ", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        string[] fieldAs = splitIgnoreCase(pair.Key, " AS ");
                        if (fieldAs.Length == 2 && field.Name.Equals(fieldAs[1].Trim()))
                        {
                            return pair.Value + ".[" + fieldAs[0] + "]";
                        }                        
                    }
                }
            }
            return prefix + "[" + dbName + "]";
        }

        private string[] splitIgnoreCase(string value, string separator)
        {
            return replaceStringAtIndex(value, separator).Split(new string[] { separator }, StringSplitOptions.None);
        } 

        private string replaceStringAtIndex(string oldValue, string newValue)
        {
            int index = oldValue.IndexOf(newValue, StringComparison.InvariantCultureIgnoreCase);
            string value = oldValue.Substring(index, newValue.Length);
            
            return oldValue.Replace(value, "").Insert(index, newValue.ToUpper());
        }

        /// <summary>
        /// Determines the Database field name that will be used in the SQL statement for the specified column
        /// </summary>
        /// <param name="entity">The entity that contains the field</param>
        /// <param name="col">the column info</param>
        /// <param name="aliases">A dictionary of database aliases</param>
        /// <returns>The database field name</returns>
        public string GetFieldName(Entity entity, ColumnInfo col, IDictionary<string, string> aliases)
        {
            if (aliases != null && aliases.ContainsKey(col.Name))
            {
                    return aliases[col.Name] + ".[" + col.Name + "]";
            }
            else if (aliases != null)
            {
                //search to see if there is a " AS " in one of the aliases keys
                foreach (KeyValuePair<string, string> pair in aliases)
                {
                    if (pair.Key.IndexOf(" AS ", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        string[] fieldAs = splitIgnoreCase(pair.Key, " AS ");
                        if (fieldAs.Length == 2 && col.Name.Equals(fieldAs[1].Trim()))
                        {
                            return pair.Value + ".[" + fieldAs[0] + "]";
                        }
                    }
                }
            }

            return "M.[" + col.Name + "]";
        }

        /// <summary>
        /// Determines the database field value that will be used in the SQL statement.
        /// </summary>
        /// <param name="entity">The entity that contains the field</param>
        /// <param name="field">The field</param>
        /// <returns>The field value</returns>
        public string GetStmtFieldValue(Entity entity, Field field)
        {
            string value = "";
            if (string.IsNullOrEmpty(field.DefaultVal))
            {                
                value = entity.GetProperty(field.Name);
            }
            else
            {
                value = field.DefaultVal;
            }

            return value;
        }

        /// <summary>
        /// Determines the database field value that will be used in the SQL query.
        /// </summary>
        /// <param name="entity">The entity that contains the field</param>
        /// <param name="field">The field</param>
        /// <returns>The field value</returns>
        public string GetFieldValue(Entity entity, Field field)
        {
            string value = "";
            if ("".Equals(field.DefaultVal))
            {
                //TODO: update this logic
                if (BS.Common.Entities.Field.DBType.Integer == field.DataType && string.IsNullOrEmpty(entity.GetProperty(field.Name)))
                {
                    value = "NULL";
                }
                else if (BS.Common.Entities.Field.DBType.Decimal == field.DataType && string.IsNullOrEmpty(entity.GetProperty(field.Name)))
                {
                    value = "NULL";
                }
                else if (BS.Common.Entities.Field.DBType.Date == field.DataType && string.IsNullOrEmpty(entity.GetProperty(field.Name)))
                {
                    value = "NULL";
                }
                else if (BS.Common.Entities.Field.DBType.DateTime == field.DataType && string.IsNullOrEmpty(entity.GetProperty(field.Name)))
                {
                    value = "NULL";
                }
                else
                {
                    string propertyVal = entity.GetProperty(field.Name);
                    if (propertyVal != null)
                    {
                        propertyVal = propertyVal.Replace("'", "''");
                    }

                    if ((BS.Common.Entities.Field.DBType.Date == field.DataType || BS.Common.Entities.Field.DBType.DateTime == field.DataType) && propertyVal.ToUpper().Contains("GETDATE"))
                    {
                        value = propertyVal;
                    }
                    else if (!string.IsNullOrEmpty(propertyVal) && propertyVal.ToUpper().Equals("NULL")) 
                    {
                        value = propertyVal.ToUpper();
                    }
                    else
                    {
                        value = "'" + propertyVal + "'";
                    }                    
                }
            }
            else
            {
                value = field.DefaultVal;
            }

            return value;
        }

        /// <summary>
        /// Returns the table name from the specified entity,
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <returns>the table name</returns>
        public string GetTableName(Entity entity) {
            return "[" + entity.GetTableName().Replace("[", "").Replace("]", "").Replace(".", "].[") + "]";
        }

        /// <summary>
        /// wraps the specified field with the Encrypt function and append it to the specified statement
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <param name="query">the statement</param>
        /// <param name="field">the field</param>
        public void AppendEncryptByKey(Entity entity, StringBuilder query, Field field)
        {
            if (!String.IsNullOrEmpty(this.Certificate) && !String.IsNullOrEmpty(this.EncryptionKey))
            {
                query.Append("EncryptByKey (Key_GUID('").Append(this.EncryptionKey).Append("'),CONVERT(varchar,'").Append(GetStmtFieldValue(entity, field)).Append("')),");
            }
            else
            {
                throw new Exception("EncryptionKey or Certificate NOT found. Cannot save encrypted field");
            }
        }

        /// <summary>
        /// wraps the specified field with the Decrypt function and append it to the specified statement
        /// </summary>
        /// <param name="query">the statement</param>
        /// <param name="aliases">the list of tables aliases</param>
        /// <param name="field">the field to be wrap</param>
        public void AppendDecryptByKey(StringBuilder query, IDictionary<string, string> aliases, Field field)
        {
            if (!String.IsNullOrEmpty(this.Certificate) && !String.IsNullOrEmpty(this.EncryptionKey))
            {
                query.Append("CONVERT(VARCHAR(100),DecryptByKey( ").Append(GetFieldName(field, aliases)).Append(")) AS ").Append(GetFieldName(field)).Append(",");
            }
            else
            {   //instead of throwing exception return field with message
                query.Append(" 'Unable To Decrypt Field' AS").Append(GetFieldName(field)).Append(",");
            }
        }

        /// <summary>
        /// Adds  Open and Close Symetric key statements to SQL select, update or insert
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <param name="query"> the query of stringbuilder type passed by reference for appending </param>
        public void WrapEncryptKey(Entity entity, StringBuilder query)
        {
            //Check if entity actually have a field of type encryption that is updtabale and/or insertable
            bool containEncryptField = entity.GetFields().Exists(x => x.DataType == Field.DBType.Encrypt && (x.Insertable || x.Updatable));

            if (containEncryptField && (query.ToString().ToLower().Contains("cryptbykey")))
            {
                if (!String.IsNullOrEmpty(this.Certificate) && !String.IsNullOrEmpty(this.EncryptionKey))
                {
                    StringBuilder hdr = new StringBuilder();
                    hdr.Append("OPEN SYMMETRIC KEY ").Append(this.EncryptionKey).Append(" DECRYPTION BY CERTIFICATE ").Append(this.Certificate).Append(";  ");
                    query.Insert(0, hdr);
                    query.Append(" CLOSE SYMMETRIC KEY ").Append(this.EncryptionKey);
                }
                else
                {
                    //just log the error not throw exception at this point);
                    LoggerHelper.Warning("EncryptionKey or Certificate NOT found but there are fields with internal encrypt datatype");
                }
            }
        }
    }
}
