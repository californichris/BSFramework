using System;
using System.Collections.Generic;

namespace BS.Common.Entities
{
    /// <summary>
    /// Entity is the fundamental unit of data storage. 
    /// <para>It has a set of zero or more properties and a list
    /// of <see cref="T:BS.Common.Entities.Field"/> fields that defined each property.</para>
    /// 
    /// <para>Represents a record in the specified tableName.</para>
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public class Entity
    {
        private IDictionary<string, string> propertyMap = new Dictionary<string,string>();
        private List<Field> fields = new List<Field>();

        private string tableName;

        /// <summary>
        /// Table Name accessor
        /// </summary>
        /// <returns>The entity table name</returns>
        public string GetTableName()
        {
            return this.tableName;
        }

        /// <summary>
        /// Table Name accessor
        /// </summary>
        /// <param name="tableName">The table name</param>
        public void SetTableName(string tableName)
        {
            this.tableName = tableName;
        }

        /// <summary>
        /// Creates a generic type entity.
        /// </summary>
        public Entity():this("GenericType")
        {

        }

        /// <summary>
        /// Create a new Entity for the specified table
        /// </summary>
        /// <param name="tableName">Table name</param>
        public Entity(string tableName)
        {
            this.tableName = tableName;
        }

        /// <summary>
        /// Sets the propertyName to value.
        /// 
        /// If the property have been set before will be updated.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="value">The value of the property</param>
        public void SetProperty(string propertyName, string value)
        {
            propertyMap[propertyName] = value;
        }

        /// <summary>
        /// Gets the property corresponding to propertyName.
        /// 
        /// If the property is not found return an empty string.
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <returns>The entity property</returns>
        public string GetProperty(string propertyName)
        {
            if (propertyMap.ContainsKey(propertyName))
            {
                return propertyMap[propertyName];
            }
            return "";
        }

        /// <summary>
        /// Gets all of the properties belonging to this Entity.
        /// </summary>
        /// <returns>The entity properties</returns>
        public IDictionary<string, string> GetProperties() {
            return this.propertyMap;
        }

        /// <summary>
        /// Sets the specified entity properties.
        /// </summary>
        /// <param name="properties">The entity properties</param>
        public void SetProperties(IDictionary<string, string> properties)
        {
            this.propertyMap = properties;
        }

        /// <summary>
        /// Adds the specified field to the entity field list.
        /// </summary>
        /// <param name="field">The entity field</param>
        /// <exception cref="System.ArgumentNullException">Thrown when field is null or if the field does not have a name.</exception>
        public void SetField(Field field)
        {
            if (field == null) throw new ArgumentNullException("Cannot add a null field.");
            if (string.IsNullOrEmpty(field.Name)) throw new ArgumentNullException("Cannot add a field without a name.");
            
            Field current = GetField(field.Name);
            if (current != null)
            {
                fields.Remove(current);
            }

            fields.Add(field);
        }

        /// <summary>
        /// Gets the entity Id value or NULL otherwise
        /// </summary>
        /// <returns>The entity id value</returns>
        public string GetEntityId()
        {
            Field field = fields.Find(x => x.Id);
            if(field == null) {
                return null;
            } else {
                return GetProperty(field.Name);
            }
        }

        /// <summary>
        /// Gets the field flag has id or NULL otherwise
        /// </summary>
        /// <returns>The entity field flag as id</returns>
        public Field GetFieldId()
        {
            return fields.Find(x => x.Id);
        }

        /// <summary>
        /// Gets the field with the specified name or NULL otherwise
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The entity field</returns>
        public Field GetField(string name)
        {
            return fields.Find(x => x.Name.Equals(name));
        }

        /// <summary>
        /// Gets all of the fields belonging to this Entity.
        /// </summary>
        /// <returns>The list of fields</returns>
        public List<Field> GetFields()
        {
            return this.fields;
        }

        /// <summary>
        /// Sets the entity fields 
        /// </summary>
        /// <param name="fields">The list of fields</param>
        public void SetFields(List<Field> fields)
        {
            this.fields = fields;
        }
    }
}