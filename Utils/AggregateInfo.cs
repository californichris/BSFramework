using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BS.Common.Utils
{
    public class AggregateInfo
    {
        private IList<AggregateFunc> _functions = null;

        public string GroupByFields { get; set; }

        public string GroupingField { get; set; }

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

        public string[] GetGroupByFields()
        {
            if (string.IsNullOrEmpty(this.GroupByFields)) return new string[] { };

            return this.GroupByFields.Split(',');
        }
    }

    public class AggregateFunc
    {
        private string _alias;

        public AggregateFunc()
        {
        }

        public AggregateFunc(string func, string fieldName)
            : this(func, fieldName, "Aggregate")
        {
        }

        public AggregateFunc(string func, string fieldName, string alias)
            : this(func, fieldName, alias, "", "")
        {
        }

        public AggregateFunc(string func, string fieldName, string alias, string havingOperator, string havingValue)
        {
            this.Function = func;
            this.FieldName = fieldName;
            this.Alias = alias;
            this.HavingOperator = havingOperator;
            this.HavingValue = havingValue;
        }

        public string Function { get; set; }
        
        public string FieldName { get; set; }

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

        public string HavingOperator { get; set; }

        public string HavingValue { get; set; }
    }
}
