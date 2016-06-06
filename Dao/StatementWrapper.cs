using System.Collections.Generic;
using System.Text;

namespace BS.Common.Dao
{
    /// <summary>
    /// Represents an SQL parameterized statement. It contains a parameterized statement as well as the parameters need to execute the statement.
    /// </summary>
    public class StatementWrapper
    {
        private IList<DBParam> _DBParams;

        /// <summary>
        /// The parameterized query.
        /// </summary>
        public StringBuilder Query { get; set; }
        
        /// <summary>
        /// The list of parameters required to execute the query.
        /// </summary>
        public IList<DBParam> DBParams
        {
            get
            {

                if (_DBParams == null)
                {
                    _DBParams = new List<DBParam>();
                }

                return this._DBParams;
            }

            set { this._DBParams = value; }
        }

        public StatementWrapper()
        {

        }

        public StatementWrapper(StringBuilder query, IList<DBParam> dbParams)
        {
            this.Query = query;
            this.DBParams = dbParams;
        }
    }
}
