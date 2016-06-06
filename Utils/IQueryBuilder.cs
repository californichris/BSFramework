using System;
using System.Collections.Generic;
using System.Text;
using BS.Common.Dao;
using BS.Common.Entities;

namespace BS.Common.Utils
{
    /// <summary>
    /// This interface defines the query builder related operations.
    /// </summary>
    public interface IQueryBuilder
    {
        /// <summary>
        /// Builds a SELECT statement using the specified entity.
        /// <para>If there is fields with a ForeignKey specified the resulting statement will have the specified JOIN clause to the ForeignKey.TableName</para>
        /// <para>also the ForeignKey.JoinFields will be included in the SELECT if present.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The SELECT statement</returns>
        StringBuilder BuildQuery(Entity entity);

        /// <summary>
        /// Builds a SELECT statement using the specified entity and filter.
        /// <para>If there is fields with a ForeignKey specified the resulting statement will have the specified JOIN clause to the ForeignKey.TableName</para>
        /// <para>also the ForeignKey.JoinFields will be included in the SELECT if present.</para>
        /// <para>The FilterInfo is used to build the WHERE clause of the statement as well as to narrow the results to the specified length.</para>
        /// </summary>
        /// <param name="entity">An Entity, that contains all the table data to build the statement</param>
        /// <param name="filter">A FilterInfo, that contains the data to build the WHERE clause</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The SELECT statement</returns>
        StringBuilder BuildQuery(Entity entity, FilterInfo filter);

        /// <summary>
        /// Creates a SELECT statement using the specified entity and filter.
        /// The where section is created from the entity properties and the filter is used for the sort section.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="filter">The filter info</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The query</returns>
        /// <returns>The SELECT statement</returns>
        StringBuilder BuildFindEntitiesQuery(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType);

        /// <summary>
        /// Creates the filtered total query of the specified entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="filter">The filter</param>
        /// <returns>The query</returns>
        /// <returns>The SELECT statement</returns>
        StringBuilder BuildFilteredTotalRecordsQuery(Entity entity, FilterInfo filter);

        /// <summary>
        /// Creates the where section of a SELECT statement from the specified entity and filter and appends the result to the specified query.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="filter">The join part of the query</param>
        /// <param name="query">The query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>        
        void BuildQueryWhereSection(Entity entity, FilterInfo filter, StringBuilder query, IDictionary<string, string> aliases);

        /// <summary>
        /// Creates the select list of a SELECT statement from the specified entity and appends the result to the specified query.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="query">The query</param>
        /// <param name="join">The join part of the query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>
        void BuildQueryFieldsSection(Entity entity, StringBuilder query, StringBuilder join, IDictionary<string, string> aliases);

        /// <summary>
        /// Creates the select list of a SELECT statement from the specified entity and appends the result to the specified query.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="query">The query</param>
        /// <param name="join">The join part of the query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>
        /// <param name="onlyJoin">Signals if the method will append the field section or not</param>
        void BuildQueryFieldsSection(Entity entity, StringBuilder query, StringBuilder join, IDictionary<string, string> aliases, bool onlyJoin);

        /// <summary>
        /// Builds an INSERT statement using the specified entity.
        /// <para>Fields flag as not insertable will not be included in the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The INSERT statement</returns>
        StringBuilder BuildInsertQuery(Entity entity);

        /// <summary>
        /// Builds an UPDATE statement using the specified entity. 
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The UPDATE statement</returns>
        StringBuilder BuildUpdateQuery(Entity entity);

        /// <summary>
        /// Builds an UPDATE statement using the specified entity properties. 
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The UPDATE statement</returns>
        StringBuilder BuildUpdateEntityQuery(Entity entity);

        /// <summary>
        /// Builds a DELETE statement using the specified entity.
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The DELETE statement</returns>
        StringBuilder BuildDeleteQuery(Entity entity);

        /// <summary>
        /// Builds an INSERT statement using the specified instance.
        /// </summary>
        /// <param name="instance">An Object instance</param>
        /// <param name="idField">The id field</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The INSERT statement</returns>
        StringBuilder BuildInsertQuery(Object instance, string idField);

        /// <summary>
        /// Builds an INSERT statement using the specified instance.
        /// </summary>
        /// <param name="instance">An Object instance</param>
        /// <param name="idField">The id field</param>
        /// <param name="defaultVals">Optional dictionary that contains default values for certain fields.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The INSERT statement</returns>
        StringBuilder BuildInsertQuery(Object instance, string idField, IDictionary<string, string> defaultVals);
        
        /// <summary>
        /// Builds an SQL INSERT statement using the specified instance.
        /// </summary>
        /// <param name="instance">An Object instance</param>
        /// <param name="idField">The id field</param>
        /// <param name="defaultVals">Optional dictionary that contains default values for certain fields.</param>
        /// <param name="outputId">Flag to indicate the inclusion (true) or exclusion (false) of the inserted id field</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The INSERT statement</returns>
        StringBuilder BuildInsertQuery(Object instance, string idField, IDictionary<string, string> defaultVals, bool outputId);

        /// <summary>
        /// Builds an UPDATE statement using the specified instance. 
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="idField">The id field</param>
        /// <exception cref="System.ArgumentNullException">Thrown when instance is null</exception>
        /// <returns>The UPDATE statement</returns>
        StringBuilder BuildUpdateQuery(Object instance, string idField);

        /// <summary>
        /// Builds an UPDATE statement using the specified instance. 
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="idField">The id field</param>
        /// <param name="defaultVals">Optional dictionary that contains default values for certain fields.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when instance is null</exception>
        /// <returns>The UPDATE statement</returns>
        StringBuilder BuildUpdateQuery(Object instance, string idField, IDictionary<string, string> defaultVals);

        /// <summary>
        /// Builds a DELETE statement using the specified instance.
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <param name="idField">The id field</param>
        /// <returns>The DELETE statement</returns>
        StringBuilder BuildDeleteQuery(Object instance, string idField);

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
        StatementWrapper BuildSelectStatement(Entity entity, FilterInfo filter);

        /// <summary>
        /// Creates a SELECT statement using the specified entity and filter.
        /// The where section is created from the entity properties and the filter is used for the sort section.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="filter">The filter info</param>
        /// <param name="searchType">The search type</param>
        /// <returns>The query</returns>
        /// <returns>The Statement Wrapper containing the SELECT statement and the statement parameters.</returns>
        StatementWrapper BuildFindEntitiesStatement(Entity entity, FilterInfo filter, FilterInfo.SearchType searchType);

        /// <summary>
        /// Creates the filtered total query of the specified entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="filter">The filter</param>
        /// <returns>The Statement Wrapper containing the SELECT statement and the statement parameters.</returns>
        StatementWrapper BuildFilteredTotalRecordsStatement(Entity entity, FilterInfo filter);

        /// <summary>
        /// Builds an INSERT statement using the specified entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The Statement Wrapper containing the INSERT statement and the statement parameters.</returns>
        StatementWrapper BuildInsertStatement(Entity entity);

        /// <summary>
        /// Builds an UPDATE statement using the specified entity. 
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when instance is null</exception>
        /// <returns>The Statement Wrapper containing the UPDATE statement and the statement parameters.</returns>
        StatementWrapper BuildUpdateStatement(Entity entity);

        /// <summary>
        /// Builds an UPDATE statement using the specified entity properties. 
        /// <para>The Field flag as the entitiy Id is used in the WHERE clause of the statement.</para>
        /// <para>Fields flag as not updatable are not included in the statement.</para>
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <exception cref="System.ArgumentNullException">Thrown when entity is null, entity TableName is not specified.</exception>
        /// <returns>The Statement Wrapper containing the UPDATE statement and the statement parameters.</returns>
        StatementWrapper BuildUpdateEntityStatement(Entity entity);

        /// <summary>
        /// Builds a DELETE statement using the specified entity.
        /// </summary>
        /// <param name="entity">An Entity</param>
        /// <returns>The Statement Wrapper containing the DELETE statement and the statement parameters.</returns>
        StatementWrapper BuildDeleteStatement(Entity entity);

        StatementWrapper BuildAggregateStatement(Entity entity, AggregateInfo aggregateInfo, Entity aggregateEntity, FilterInfo.SearchType searchType, FilterInfo filter);

        /// <summary>
        /// Creates the where section of a SELECT statement from the specified entity and filter and appends the result to the specified query also adds the
        /// statement parameters to the specified queryParams list.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="filter">The join part of the query</param>
        /// <param name="query">The query</param>
        /// <param name="aliases">A dictionary of the query aliases</param>
        /// <param name="queryParams">The query params list</param>
        void BuildQueryWhereSection(Entity entity, FilterInfo filter, StringBuilder query, IDictionary<string, string> aliases, IList<DBParam> queryParams);

        /// <summary>
        /// Determines the database field name that will be used in the statement for the specified field
        /// </summary>
        /// <param name="field">The Field</param>
        /// <returns>The database field name</returns>
        string GetFieldName(Field field);

        /// <summary>
        /// Determines the database field name that will be used in the statement for the specified field
        /// </summary>
        /// <param name="field">The Field</param>
        /// <param name="aliases">A dictionary of database aliases</param>
        /// <returns>The database field name</returns>
        string GetFieldName(Field field, IDictionary<string, string> aliases);

        /// <summary>
        /// Determines the Database field name that will be used in the statement for the specified column
        /// </summary>
        /// <param name="entity">The entity that contains the field</param>
        /// <param name="col">the column info</param>
        /// <param name="aliases">A dictionary of database aliases</param>
        /// <returns>The database field name</returns>
        string GetFieldName(Entity entity, ColumnInfo col, IDictionary<string, string> aliases);

        /// <summary>
        /// Determines the database field value that will be used in the statement.
        /// </summary>
        /// <param name="entity">The entity that contains the field</param>
        /// <param name="field">The field</param>
        /// <returns>The field value</returns>
        string GetFieldValue(Entity entity, Field field);

        string GetTableName(Entity entity);
    }
}
