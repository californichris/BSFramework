using System.Collections.Generic;
using System.Text;
using BS.Common.Entities;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using BS.Common.Dao;

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
    public class QueryBuilder : IQueryBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly string Param = "@p";

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
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

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
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

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
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

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
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

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
            if (entity == null || entity.GetFieldId() == null)
            {
                LoggerHelper.Error("Entity does not have an Id field defined.");
                throw new ArgumentNullException("Entity is null or does not have an Id field defined.");
            }

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
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

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
            if (instance == null)
            {
                LoggerHelper.Warning("An instance must be specified.");
                throw new ArgumentNullException("An instance must be specified.");
            }

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
                    fieldValues.Append("'").Append(prop.GetValue(instance, null)).Append("',");
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
            return BuildUpdateQuery(instance, idField);
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
            if (instance == null)
            {
                LoggerHelper.Warning("An instance must be specified.");
                throw new ArgumentNullException("An instance must be specified.");
            }

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
            }

            query.Remove(query.Length - 1, 1);
            query.Append(" WHERE ");
            query.Append(idField).Append(" = ").Append(idValue);

            return query;
        }

        public StringBuilder BuildDeleteQuery(Object instance, string idField)
        {
            if (instance == null)
            {
                LoggerHelper.Warning("An instance must be specified.");
                throw new ArgumentNullException("An instance must be specified.");
            }
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

        public StatementWrapper BuildInsertStatement(Entity entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();
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

            return new StatementWrapper(query, queryParams);
        }

        public StatementWrapper BuildUpdateStatement(Entity entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();

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

            return new StatementWrapper(query, queryParams);
        }

        public StatementWrapper BuildUpdateEntityStatement(Entity entity)
        {
            if (entity == null || entity.GetFieldId() == null)
            {
                LoggerHelper.Error("Entity does not have an Id field defined.");
                throw new ArgumentNullException("Entity is null or does not have an Id field defined.");
            }

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();

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

            return new StatementWrapper(query, queryParams);
        }

        public StatementWrapper BuildDeleteStatement(Entity entity)
        {
            if (entity == null || string.IsNullOrEmpty(entity.GetTableName()))
            {
                LoggerHelper.Warning("Entity is null or entity TableName is not specified.");
                throw new ArgumentNullException("Entity is null or entity TableName is not specified.");
            }

            StringBuilder query = new StringBuilder();
            IList<DBParam> queryParams = new List<DBParam>();
            query.Append("DELETE FROM ").Append(GetTableName(entity)).Append(" WHERE ");
            query.Append(GetFieldName(entity.GetFieldId())).Append(" = ").Append(Param + queryParams.Count);
            queryParams.Add(new DBParam(queryParams, entity.GetEntityId(), entity.GetFieldId().GetDbType()));

            return new StatementWrapper(query, queryParams);
        }

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

            return new StatementWrapper(query, queryParams); ;
        }

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

            return new StatementWrapper(query, queryParams);
        }

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
                    }

                    AppendLikeField(orWhere, fieldName, filter.Search, "OR", queryParams);
                }

                if (!string.IsNullOrEmpty(col.Search)) {
                    string value = col.Search;
                    // the following logic is to support backward compatibility before calling AppendSearchCondition
                    if (col.SearchType == FilterInfo.ColumnSearchType.LIKE && value.IndexOf("_RANGE_") == -1 && !value.StartsWith("LIST_")) value = "LIKE_" + value;
                    if (col.SearchType == FilterInfo.ColumnSearchType.NULL)
                    {
                        value = value == "NULL" ? "NULL" : "NOT_NULL";
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

        public void AppendSearchCondition(StringBuilder where, Field field, string fieldName, string value, string type, IDictionary<string, string> aliases, IList<DBParam> queryParams)
        {
            if (field != null)
            {
                fieldName = GetFieldName(field, aliases);
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
                    AppendRange(where, fieldName, value, queryParams, field);
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
                queryParams.Add(new DBParam(queryParams, value, field.GetDbType()));
            }
            
            where.Append(type);
        }

        public void AppendField(StringBuilder where, string fieldName, string value, string type)
        {
            AppendField(where, fieldName, value, type, null, null);
        }

        public void AppendRange(StringBuilder query, string fieldName, string value, IList<DBParam> queryParams, Field field)
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
            query.Append(") AND");
        }

        public void AppendRange(StringBuilder query, string fieldName, string value)
        {
            AppendRange(query, fieldName, value, null, null);
        }

        public void AppendList(StringBuilder where, string fieldName, string value, string type, IList<DBParam> queryParams)
        {
            value = replaceStringAtIndex(value, "LIST_").Replace("LIST_", "");
            if (queryParams == null)
            {
                where.Append(fieldName).Append(" IN (").Append(value).Append(") ").Append(type).Append(" ");
                return;
            }
            
            where.Append(fieldName).Append(" IN (");
            string[] list = value.Split(new string[] { "," }, StringSplitOptions.None);
            foreach (string val in list)
            {
                where.Append(Param + queryParams.Count).Append(",");
                queryParams.Add(new DBParam(queryParams, val, DbType.String, false));
            }

            if (list.Length > 0) where.Remove(where.Length - 1, 1);

            where.Append(") ").Append(type).Append(" ");
        }

        public void AppendList(StringBuilder where, string fieldName, string value, string type)
        {
            AppendList(where, fieldName, value, type, null);            
        }

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
    

        public string GetTableName(Entity entity) {
            return "[" + entity.GetTableName().Replace("[", "").Replace("]", "").Replace(".", "].[") + "]";
        }
    }
}
