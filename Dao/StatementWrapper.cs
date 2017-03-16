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

        /// <summary>
        /// Creates an empty StatementWrapper instance
        /// </summary>
        public StatementWrapper()
        {
        }

        /// <summary>
        /// Creates an StatementWrapper instance with the specified query and dbParams
        /// </summary>
        /// <param name="query">The parameterized query.</param>
        /// <param name="dbParams">The list of parameters required to execute the query.</param>
        public StatementWrapper(StringBuilder query, IList<DBParam> dbParams)
        {
            this.Query = query;
            this.DBParams = dbParams;
        }
    }
}
