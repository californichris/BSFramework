using System.Collections.Generic;

namespace BS.Common.Utils
{
    /// <summary>
    /// Contains the necessary information used by <see cref="T:EPE.Common.Utils.IQueryBuilder"/> to build 
    /// an Aggregate statement.
    /// </summary>
    /// <history>
    ///     <change date="12/01/2016" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public class AggregateInfo
    {

        private IList<AggregateFunc> _functions = null;

        /// <summary>
        /// Comma separated values of the fields used in the GROUP Clause of the aggregated functions
        /// </summary>
        public string GroupByFields { get; set; }

        /// <summary>
        /// Indicates whether a specified column expression in a GROUP BY list is aggregated or not 
        /// </summary>
        public string GroupingField { get; set; }

        /// <summary>
        /// Aggregate Functions list 
        /// </summary>
        public IList<AggregateFunc> Functions
        {
            get
            {

                if (_functions == null)
                {
                    _functions = new List<AggregateFunc>();
                }

                return this._functions;
            }

            set { _functions = value; }
        }

        /// <summary>
        /// returns an string array  of tje group by fields
        /// </summary>
        public string[] GetGroupByFields()
        {
            if (string.IsNullOrEmpty(this.GroupByFields)) return new string[] { };

            return this.GroupByFields.Split(',');
        }
    }

    /// <summary>
    /// Represents an Aggregated Function
    /// </summary>
    public class AggregateFunc
    {
        private string _alias;

        /// <summary>
        /// Creates an empty AggregateFunc.
        /// </summary>
        public AggregateFunc()
        {
        }

        /// <summary>
        /// Creates an AggregateFunc instance with the specified funct and fieldName.
        /// </summary>
        /// <param name="func">The aggregated function type (COUNT,SUM,MAX...)</param>
        /// <param name="fieldName">The field in which the aggregated function will be performed</param>
        public AggregateFunc(string func, string fieldName)
            : this(func, fieldName, "Aggregate")
        {
        }

        /// <summary>
        /// Creates an AggregateFunc instance with the specified funct, fieldName an alias.
        /// </summary>
        /// <param name="func">The aggregated function type (COUNT,SUM,MAX...)</param>
        /// <param name="fieldName">The field in which the aggregated function will be performed</param>
        /// <param name="alias">The alias name of the return column</param>
        public AggregateFunc(string func, string fieldName, string alias)
            : this(func, fieldName, alias, "", "")
        {
        }

        /// <summary>
        /// Creates an AggregateFunc instance with the specified funct, fieldName, alias, havingOperator and havingValue.
        /// </summary>
        /// <param name="func">The aggregated function type (COUNT,SUM,MAX...)</param>
        /// <param name="fieldName">The field in which the aggregated function will be performed</param>
        /// <param name="alias">The alias name of the return column</param>
        /// <param name="havingOperator">The having operator</param>
        /// <param name="havingValue">The having value</param>
        public AggregateFunc(string func, string fieldName, string alias, string havingOperator, string havingValue)
        {
            this.Function = func;
            this.FieldName = fieldName;
            this.Alias = alias;
            this.HavingOperator = havingOperator;
            this.HavingValue = havingValue;
        }

        /// <summary>
        /// The aggregated function type (COUNT,SUM,MAX...)
        /// </summary>
        public string Function { get; set; }
        
        /// <summary>
        /// The field in which the aggregated function will be performed in case of a sum * can be used
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The alias name of the return column, if not specified the function will be return as Aggregated#
        /// </summary>
        public string Alias { 
            get
            {
                if (string.IsNullOrEmpty(this._alias)) return "Aggregate";
                
                return this._alias;
            }
            
            set
            {
                this._alias = value;
            } 
        }

        /// <summary>
        /// The operator used in the search condition for an aggregate.
        /// </summary>
        public string HavingOperator { get; set; }

        /// <summary>
        /// The value used in the search condition for an aggregate.
        /// </summary>
        public string HavingValue { get; set; }
    }
}
