using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using BS.Common.Entities;
using BS.Common.Utils;


namespace BS.Common.Dao
{
    /// <summary>
    /// An Entity type implementation of the RowProcessor.
    /// </summary>
    public class EntityRowProcessor : RowProcessor
    {
        /// <summary>
        /// The default datetime format use to format a database datetime type value
        /// </summary>
        public static readonly string DefaultDateTimeFormat = "MM/dd/yyyy HH:mm:ss";

        /// <summary>
        /// The default date format use to format a database date type value
        /// </summary>
        public static readonly string DefaultDateFormat = "MM/dd/yyyy";

        /// <summary>
        /// The application setting key to override the default datetime format.
        /// </summary>
        public static readonly string DateTimeFormatKey = "DateTimeFormat";

        /// <summary>
        /// The application setting key to override the default date format.
        /// </summary>
        public static readonly string DateFormatKey = "DateFormat";

        private Entity entityType;

        /// <summary>
        /// Creates an instance of the processor with the specified entity instance.
        /// The entity instance is the one used to create the Entities return by the
        /// ToBean and ToBeanList methods not the type arguments, because what differ 
        /// one Entity from another are the instance fields not the type.
        /// </summary>
        /// <param name="entityType">The type of the entity</param>
        public EntityRowProcessor(Entity entityType)
        {
            this.entityType = entityType;
        }

        /// <summary>
        /// Convert a ResultSet row into an Entity.
        /// </summary>
        /// <typeparam name="T">The type of entity to create</typeparam>
        /// <param name="rs">ResultSet that supplies the bean data</param>
        /// <param name="type">Class from which to create the entity instance</param>
        /// <returns>The newly created entity</returns>
        public T ToBean<T>(DbDataReader rs, Type type)
        {
            string datetimeFormat = DefaultDateTimeFormat;
            string dateFormat = DefaultDateFormat;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[DateTimeFormatKey]))
            {
                datetimeFormat = ConfigurationManager.AppSettings[DateTimeFormatKey];
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[DateFormatKey]))
            {
                dateFormat = ConfigurationManager.AppSettings[DateFormatKey];
            }

            DataTable metadata = rs.GetSchemaTable();
            List<string> cols = metadata.Rows.Cast<DataRow>().Select(row => row[BasicRowProcesor.ColumnName] as string).ToList();


            Entity instance = (Entity) Activator.CreateInstance(this.entityType.GetType());
            foreach (Field field in this.entityType.GetFields()) 
            {
                if (!cols.Contains(field.DBName))
                {
                    LoggerHelper.Warning("Field " + field.Name + " (DBName = " + field.DBName + ") was not on the resultset returning empty string");
                    instance.SetProperty(field.Name, "");
                    continue;
                }

                if (field.DataType == Field.DBType.DateTime)
                {
                    string value = getValue(rs, field);
                    if (!string.IsNullOrEmpty(value))
                    {
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(value, out dt))
                        {
                            value = dt.ToString(datetimeFormat);
                        }
                    }

                    instance.SetProperty(field.Name, value);
                }
                else if (field.DataType == Field.DBType.Date)
                {
                    string value = getValue(rs, field);
                    if (!string.IsNullOrEmpty(value))
                    {
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(value, out dt))
                        {
                            value = dt.ToString(dateFormat);
                        }
                    }
                    instance.SetProperty(field.Name, value);
                }
                else
                {
                    instance.SetProperty(field.Name, getValue(rs, field));
                }
            }

            return (T) Convert.ChangeType(instance, this.entityType.GetType());
        }

        /// <summary>
        /// Convert a ResultSet into a List of Entities.
        /// </summary>
        /// <typeparam name="T">The type of entity to create</typeparam>
        /// <param name="rs">ResultSet that supplies the entity data</param>
        /// <param name="type">Type Class from which to create the entity instance</param>
        /// <returns>A List of entities with the given type in the order they were returned by the ResultSet.</returns>
        public IList<T> ToBeanList<T>(DbDataReader rs, Type type)
        {
            IList<T> list = new List<T>();
            while (rs.Read())
            {
                list.Add(this.ToBean<T>(rs, type));
            }

            return list;
        }

        /// <summary>
        /// Returns the resultset column value
        /// </summary>
        /// <param name="rs">The resultset</param>
        /// <param name="field">field that will be return</param>
        /// <returns>The column value</returns>
        private string getValue(DbDataReader rs, Field field)
        {
            return rs[field.DBName].ToString();
        }
    }
}
