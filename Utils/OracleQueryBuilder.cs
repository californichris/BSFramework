using System.Collections.Generic;
using System.Text;
using BS.Common.Entities;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using BS.Common.Dao;

namespace BS.Common.Utils
{
    /// <summary>
    ///     Utility class that generates queries base on a Entity object.
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public class OracleQueryBuilder : IQueryBuilder
    {
        /// <summary>
        /// Builds an Oracle SELECT statement using the specified entity.
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
            query.Append(" FROM ").Append(entity.GetTableName()).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            return query;
        }

        /// <summary>
        /// Builds an Oracle SELECT statement using the specified entity and filter.
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
            query.Append(") AS RowNumber FROM ").Append(entity.GetTableName()).Append(" M ");

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
        /// Creates an Oracle SELECT statement using the specified entity and filter.
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
            query.Append(" FROM ").Append(entity.GetTableName()).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }


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
                    string fieldName = pair.Key;
                    Field field = entity.GetField(pair.Key);
                    if (field != null)
                    {
                        fieldName = GetFieldName(field, aliases);
                    }
                    where.Append(fieldName).Append(" = '").Append(pair.Value).Append("' ").Append(type).Append(" ");
                }
            }

            if (where.Length > 0)
            {
                where.Remove(where.Length - (type.Length + 1), (type.Length + 1));
                query.Append(" WHERE ").Append(where);
            }

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
            query.Append(" FROM ").Append(entity.GetTableName()).Append(" M ");

            if (join.Length > 0)
            {
                query.Append(join);
            }

            BuildQueryWhereSection(entity, filter, query, aliases);

            return query;
        }

        /// <summary>
        /// Creates the where section of an Oracle SELECT statement from the specified entity and filter and appends the result to the specified query.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="filter">The join part of the query</param>
        /// <param name="query">The query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>        
        public void BuildQueryWhereSection(Entity entity, FilterInfo filter, StringBuilder query, IDictionary<string, string> aliases)
        {
            StringBuilder where = new StringBuilder();
            StringBuilder andWhere = new StringBuilder();
            StringBuilder orWhere = new StringBuilder();

            foreach (ColumnInfo col in filter.Columns)
            {
                if (col.Searchable && !"".Equals(filter.Search))
                {
                    string fieldName = GetFieldName(entity, col, aliases);
                    Field field = entity.GetField(col.Name);
                    if (field != null)
                    {
                        fieldName = GetFieldName(field, aliases);
                        switch (field.DataType)
                        {
                            case Field.DBType.Date:
                                orWhere.Append(" CONVERT(VARCHAR,").Append(fieldName).Append(", 101) ");
                                break;
                            default:
                                orWhere.Append(" ").Append(fieldName);
                                break;
                        }
                    }
                    else
                    {
                        orWhere.Append(" ").Append(fieldName);
                    }

                    if (col.SearchType == FilterInfo.ColumnSearchType.LIKE)
                    {
                        orWhere.Append(" LIKE '%").Append(filter.Search).Append("%' OR");
                    }
                    else //Equals
                    {
                        orWhere.Append(" = '").Append(filter.Search).Append("' OR");
                    }
                }

                if (col.Searchable && !string.IsNullOrEmpty(col.Search))
                {
                    string fieldName = GetFieldName(entity, col, aliases);
                    Field field = entity.GetField(col.Name);
                    if (field != null)
                    {
                        fieldName = GetFieldName(field, aliases);
                    }

                    if (col.Search.IndexOf("_RANGE_") == -1)
                    {
                        if (col.SearchType == FilterInfo.ColumnSearchType.LIKE)
                        {
                            andWhere.Append(" ").Append(fieldName).Append(" LIKE '%").Append(col.Search).Append("%' AND");
                        }
                        else
                        {
                            andWhere.Append(" ").Append(fieldName).Append(" = '").Append(col.Search).Append("' AND");
                        }
                    }
                    else
                    {
                        string[] rangeValues = col.Search.Split(new string[] { "_RANGE_" }, StringSplitOptions.None);
                        andWhere.Append(" (");

                        if (!string.IsNullOrEmpty(rangeValues[0])) andWhere.Append(fieldName).Append(" >= '").Append(rangeValues[0]).Append("'");
                        if (!string.IsNullOrEmpty(rangeValues[0]) && !string.IsNullOrEmpty(rangeValues[1])) andWhere.Append(" AND ");
                        if (!string.IsNullOrEmpty(rangeValues[1])) andWhere.Append(fieldName).Append(" <= '").Append(rangeValues[1]).Append("'");

                        andWhere.Append(") AND");
                    }

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

        /// <summary>
        /// Creates the select list of an Oracle SELECT statement from the specified entity and appends the result to the specified query.
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
        /// Creates the select list of an Oracle SELECT statement from the specified entity and appends the result to the specified query.
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

                    join.Append(" JOIN ").Append(field.ForeignKey.TableName).Append(" ").Append(joinAlias);
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
        /// Builds an Oracle INSERT statement using the specified entity.
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

            query.Append("INSERT INTO ").Append(entity.GetTableName()).Append("(");

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

            query.Append(") OUTPUT INSERTED.").Append(entity.GetFieldId().Name);
            query.Append(" VALUES(").Append(fieldValues).Append(")");

            return query;
        }

        public StatementWrapper BuildInsertStatement(Entity entity)
        {
            throw new Exception("Not implemented");
        }

        public StatementWrapper BuildUpdateStatement(Entity entity)
        {
            throw new Exception("Not implemented");
        }

        public StatementWrapper BuildUpdateEntityStatement(Entity entity)
        {
            throw new Exception("Not implemented");
        }

        public StatementWrapper BuildDeleteStatement(Entity entity)
        {
            throw new Exception("Not implemented");
        }

        public StatementWrapper BuildFilteredTotalRecordsStatement(Entity entity, FilterInfo filter)
        {
            throw new Exception("Not implemented");
        }

        public StatementWrapper BuildSelectStatement(Entity entity, FilterInfo filter)
        {
            throw new Exception("Not implemented");
        }

        public StatementWrapper BuildFindEntitiesStatement(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType)
        {
            throw new Exception("Not implemented");
        }

        public void BuildQueryWhereSection(Entity entity, FilterInfo filter, StringBuilder query, IDictionary<string, string> aliases, IList<DBParam> queryParams)
        {
            throw new Exception("Not implemented");
        }

        /// <summary>
        /// Builds an Oracle UPDATE statement using the specified entity. 
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

            query.Append("UPDATE ").Append(entity.GetTableName()).Append(" SET ");
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
        /// Builds an Oracle UPDATE statement using the specified entity properties. 
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
            query.Append("UPDATE ").Append(entity.GetTableName()).Append(" SET ");
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
        /// Builds an Oracle DELETE statement using the specified entity.
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
            query.Append("DELETE FROM ").Append(entity.GetTableName()).Append(" WHERE ");
            query.Append(GetFieldName(entity.GetFieldId())).Append(" = ").Append(entity.GetEntityId());

            return query;
        }

        /// <summary>
        /// Builds an Oracle INSERT statement using the specified instance.
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
        /// Builds an Oracle INSERT statement using the specified instance.
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
        /// Builds an Oracle INSERT statement using the specified instance.
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
        /// Builds an Oracle UPDATE statement using the specified instance. 
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
        /// Builds an Oracle UPDATE statement using the specified instance. 
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

        /// <summary>
        /// Determines the database field name that will be used in the Oracle statement for the specified field
        /// </summary>
        /// <param name="field">The Field</param>
        /// <returns>The database field name</returns>
        public string GetFieldName(Field field)
        {
            return GetFieldName(field, null);
        }

        /// <summary>
        /// Determines the database field name that will be used in the Oracle statement for the specified field
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
                return aliases[field.Name] + "." + dbName + "";
            }
            else if (aliases != null)
            {
                //search to see if there is a " AS " in one of the aliases keys
                foreach (KeyValuePair<string, string> pair in aliases)
                {
                    if (pair.Key.IndexOf(" AS ") != -1)
                    {
                        string[] fieldAs = pair.Key.Split(new string[] { " AS " }, StringSplitOptions.None);
                        if (fieldAs.Length == 2 && field.Name.Equals(fieldAs[1].Trim()))
                        {
                            return pair.Value + "." + fieldAs[0] + "";
                        }
                    }
                }
            }
            return prefix + "" + dbName + "";
        }

        /// <summary>
        /// Determines the Database field name that will be used in the Oracle statement for the specified column
        /// </summary>
        /// <param name="entity">The entity that contains the field</param>
        /// <param name="col">the column info</param>
        /// <param name="aliases">A dictionary of database aliases</param>
        /// <returns>The database field name</returns>
        public string GetFieldName(Entity entity, ColumnInfo col, IDictionary<string, string> aliases)
        {
            if (aliases != null && aliases.ContainsKey(col.Name))
            {
                return aliases[col.Name] + "." + col.Name + "";
            }
            else if (aliases != null)
            {
                //search to see if there is a " AS " in one of the aliases keys
                foreach (KeyValuePair<string, string> pair in aliases)
                {
                    if (pair.Key.IndexOf(" AS ") != -1)
                    {
                        string[] fieldAs = pair.Key.Split(new string[] { " AS " }, StringSplitOptions.None);
                        if (fieldAs.Length == 2 && col.Name.Equals(fieldAs[1].Trim()))
                        {
                            return pair.Value + "." + fieldAs[0] + "";
                        }
                    }
                }
            }

            return "M." + col.Name + "";
        }

        /// <summary>
        /// Determines the database field value that will be used in the Oracle statement.
        /// </summary>
        /// <param name="entity">The entity that contains the field</param>
        /// <param name="field">The field</param>
        /// <returns>The field value</returns>
        public string GetFieldValue(Entity entity, Field field)
        {
            string value = "";
            if ("".Equals(field.DefaultVal))
            {
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


        public StatementWrapper BuildAggregateStatement(Entity entity, AggregateInfo aggregateInfo, Entity aggregateEntity, FilterInfo.SearchType searchType, FilterInfo filter)
        {
            throw new NotImplementedException("This method is not available in offline mode.");
        }

        public string GetTableName(Entity entity)
        {
            return "[" + entity.GetTableName().Replace("[", "").Replace("]", "").Replace(".", "].[") + "]";
        }
    }
}
