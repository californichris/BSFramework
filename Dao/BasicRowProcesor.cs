using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Data.Common;

namespace BS.Common.Dao
{
    /// <summary>
    /// Basic implementation of the RowProcessor interface.
    /// </summary>
    public class BasicRowProcesor : RowProcessor
    {
        /// <summary>
        /// The name of the row column in the datatable schema metadata.
        /// </summary>
        public static readonly string ColumnName = "ColumnName";

        /// <summary>
        /// Convert a ResultSet row into a Bean.
        /// </summary>
        /// <typeparam name="T">The type of bean to create</typeparam>
        /// <param name="rs">ResultSet that supplies the bean data</param>
        /// <param name="type">Class from which to create the bean instance</param>
        /// <returns>The newly created bean</returns>
        public T ToBean<T>(DbDataReader rs, Type type)
        {
            DataTable metadata = rs.GetSchemaTable();
            List<string> cols = metadata.Rows.Cast<DataRow>().Select(row => row[ColumnName] as string).ToList();

            PropertyInfo[] props = type.GetProperties();
            Object instance = CreateBean<T>(rs, type, props, cols);

            return (T)Convert.ChangeType(instance, type);
        }

        /// <summary>
        /// Convert a ResultSet into a List of Beans.
        /// </summary>
        /// <typeparam name="T">The type of bean to create</typeparam>
        /// <param name="rs">ResultSet that supplies the bean data</param>
        /// <param name="type">Type Class from which to create the bean instance</param>
        /// <returns>A List of beans with the given type in the order they were returned by the ResultSet.</returns>
        public IList<T> ToBeanList<T>(DbDataReader rs, Type type)
        {
            IList<T> list = new List<T>();

            DataTable metadata = rs.GetSchemaTable();
            List<string> cols = metadata.Rows.Cast<DataRow>().Select(row => row[ColumnName] as string).ToList();
            PropertyInfo[] props = type.GetProperties();
            while (rs.Read())
            {
                list.Add(this.CreateBean<T>(rs, type, props, cols));
            }

            return list;
        }
        
        /// <summary>
        /// Creates a new object and initializes its fields from the ResultSet.
        /// </summary>
        /// <typeparam name="T">The type of bean to create</typeparam>
        /// <param name="rs">The result set.</param>
        /// <param name="type">The bean type (the return type of the object).</param>
        /// <param name="props">The property descriptors.</param>
        /// <param name="cols">The List of columns in the resultset</param>
        /// <returns>An initialized object.</returns>
        protected virtual T CreateBean<T>(DbDataReader rs, Type type, PropertyInfo[] props, List<string> cols)
        {

            Object instance = Activator.CreateInstance(type);

            foreach (PropertyInfo prop in props)
            {
                if (prop.PropertyType == typeof(String) && cols.Contains(prop.Name))
                {
                    prop.SetValue(instance, rs[prop.Name].ToString(), null);
                }
            }

            return (T) Convert.ChangeType(instance, type);
        }
    }
}
