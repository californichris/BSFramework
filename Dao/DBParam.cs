using System;
using System.Collections.Generic;
using System.Data;
using BS.Common.Utils;

namespace BS.Common.Dao
{
    /// <summary>
    /// Represents a data base parameter.
    /// </summary>
    public class DBParam
    {
        /// <summary>
        /// The parameter name
        /// </summary>
        public string ParamName { get; set; }
        
        /// <summary>
        /// The parameter value
        /// </summary>
        public object ParamValue { get; set; }
        
        /// <summary>
        /// The parameter type
        /// </summary>
        public System.Data.DbType ParamType { get; set; }
        
        /// <summary>
        /// Flag signaling if the parameter will be a LIKE command in order to add the % character to the param value.
        /// </summary>
        public bool IsLike { get; set; }

        /// <summary>
        /// Creates a DBParam instance.
        /// </summary>
        public DBParam() { }

        /// <summary>
        /// Creates a DBParam instance and sets the ParamName and ParamValue properties
        /// </summary>
        /// <param name="queryParams">List of queryparams use to build the parameter name</param>
        /// <param name="paramValue">The parameter value</param>
        public DBParam(IList<DBParam> queryParams, object paramValue) : this(QueryBuilder.Param + queryParams.Count, paramValue, DbType.String) { }

        /// <summary>
        /// Creates a DBParam instance and sets the ParamName, ParamValue, and IsLike properties
        /// </summary>
        /// <param name="queryParams">List of queryparams use to build the parameter name</param>
        /// <param name="paramValue">The parameter value</param>
        /// <param name="isLike">The isLike flag</param>
        public DBParam(IList<DBParam> queryParams, object paramValue, bool isLike) : this(QueryBuilder.Param + queryParams.Count, paramValue, DbType.String, isLike) { }

        /// <summary>
        /// Creates a DBParam instance and sets the ParamName, ParamValue, and ParamType properties
        /// </summary>
        /// <param name="queryParams">List of queryparams use to build the parameter name</param>
        /// <param name="paramValue">The parameter value</param>
        /// <param name="paramType">The parameter type</param>
        public DBParam(IList<DBParam> queryParams, object paramValue, DbType paramType) : this(QueryBuilder.Param + queryParams.Count, paramValue, paramType) { }

        /// <summary>
        /// Creates a DBParam instance and sets the ParamName, ParamValue, IsLike and ParamType properties
        /// </summary>
        /// <param name="queryParams">List of queryparams use to build the parameter name</param>
        /// <param name="paramValue">The parameter value</param>
        /// <param name="paramType">The parameter type</param>
        /// <param name="isLike">The isLike flag</param>
        public DBParam(IList<DBParam> queryParams, object paramValue, DbType paramType, bool isLike) : this(QueryBuilder.Param + queryParams.Count, paramValue, paramType, isLike) { }

        /// <summary>
        /// Creates a DBParam instance and sets the ParamName and ParamValue properties
        /// </summary>
        /// <param name="paramName">The parameter name</param>
        /// <param name="paramValue">The parameter value</param>
        public DBParam(string paramName, object paramValue) : this(paramName, paramValue, DbType.String) { }

        /// <summary>
        /// Creates a DBParam instance and sets the ParamName, ParamValue and ParamType properties
        /// </summary>
        /// <param name="paramName">The parameter name</param>
        /// <param name="paramValue">The parameter value</param>
        /// <param name="paramType">The parameter type</param>
        public DBParam(string paramName, object paramValue, DbType paramType) : this(paramName, paramValue, paramType, false) { }

        /// <summary>
        /// Creates a DBParam instance and sets the ParamName, ParamValue, IsLike and ParamType properties
        /// </summary>
        /// <param name="paramName">The parameter name</param>
        /// <param name="paramValue">The parameter value</param>
        /// <param name="paramType">The parameter type</param>
        /// <param name="isLike">The isLike flag</param>
        public DBParam(string paramName, object paramValue, DbType paramType, bool isLike)
        {
            this.ParamName = paramName;

            string value = (string)paramValue;
            if (System.Data.DbType.Boolean == paramType)
            {
                if ("1".Equals(value) || bool.TrueString.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.ParamValue = true;
                }
                else
                {
                    this.ParamValue = false;
                }
            }
            else if (DbType.Int32 == paramType)
            {
                this.ParamValue = string.IsNullOrEmpty(value) ? (object)DBNull.Value : int.Parse(value);
            }
            else if (DbType.Decimal == paramType)
            {
                this.ParamValue = string.IsNullOrEmpty(value) ? (object)DBNull.Value : decimal.Parse(value);
            }
            else if (((DbType.Date == paramType || DbType.DateTime == paramType) && string.IsNullOrEmpty(value)) || (!string.IsNullOrEmpty(value) && value.ToUpper().Equals("NULL")))
            {
                this.ParamValue = (object)DBNull.Value;
            }
            else
            {
                this.ParamValue = paramValue;
            }

            this.ParamType = paramType;
            this.IsLike = isLike;
        }

    }
}
