using System.Collections.Generic;

namespace BS.Common.Utils
{
    /// <summary>
    /// Contains the necessary information used by <see cref="T:BS.Common.Utils.QueryBuilder"/> to build 
    /// the WHERE clause of a SELECT statement.
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public class FilterInfo
    {

        /// <summary>
        /// List of valid search types
        /// </summary>
        public enum SearchType
        {
            /// <summary>
            /// AND search type
            /// </summary>
            AND,
            /// <summary>
            /// OR search type
            /// </summary>
            OR
        };

        /// <summary>
        /// List of valid column search types
        /// </summary>
        public enum ColumnSearchType 
        {
            /// <summary>
            /// Like column search type
            /// </summary>
            LIKE,
            /// <summary>
            /// Equals column search type
            /// </summary>
            EQUALS,
            /// <summary>
            /// Null column search type
            /// </summary>
            NULL
        };

        /// <summary>
        /// Creates an empty FilterInfo.
        /// </summary>
        public FilterInfo()
        {
        }

        /// <summary>
        /// Determines if the specified request contains the necesary information to create a 
        /// FilterInfo object
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>True if found False otherwise</returns>
        public static bool ContainsFilterInfo(System.Web.HttpRequest request) {
            //the first part of the if is for datables 1.9 
            return (!string.IsNullOrEmpty(request.Params["iDisplayStart"]) && !string.IsNullOrEmpty(request.Params["iDisplayLength"]) && !string.IsNullOrEmpty(request.Params["sColumns"]) && request.Params["sSearch"] != null) 
                //datables 1.10
                || (request.Params["filterInfo"] != null);
        }

        /// <summary>
        /// Creates a FilterInfo instance and populates it with data from the specified <see cref="T:System.Web.HttpRequest"/>.
        /// 
        /// Only works for the javascript DataTables API.
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        public FilterInfo(System.Web.HttpRequest request)
        {
            if (request.Params["filterInfo"] != null)
            {
                //create filter for datables 1.10
                return;
            }

            LoggerHelper.Debug("iDisplayStart=" + request.Params["iDisplayStart"]);

            this.Start = int.Parse(request.Params["iDisplayStart"]);
            this.Lenght = int.Parse(request.Params["iDisplayLength"]);
            this.Search = request.Params["sSearch"];
            this.ColumnsName = request.Params["sColumns"].Split(',');
            if (this.ColumnsName != null)
            {
                for (int i = 0; i < this.ColumnsName.Length; i++)
                {
                    bool searchable = false;
                    string searchCol = "";
                    FilterInfo.ColumnSearchType columnSearchType = FilterInfo.ColumnSearchType.LIKE;
                    if (!string.IsNullOrEmpty(request.Params["bSearchable_" + i]))
                    {
                        searchable = bool.Parse(request.Params["bSearchable_" + i]);
                        searchCol = request.Params["sSearch_" + i];
                    }

                    string stype = request.Params["sSearchType_" + i];

                    if (!string.IsNullOrEmpty(stype) && "equals".Equals(stype))
                    {
                        columnSearchType = FilterInfo.ColumnSearchType.EQUALS;
                    }
                    else if (!string.IsNullOrEmpty(stype) && "null".Equals(stype))
                    {
                        columnSearchType = FilterInfo.ColumnSearchType.NULL;
                    }

                    string name = request.Params["mDataProp_" + i];
                    this.Columns.Add(new ColumnInfo(this.ColumnsName[i], name, searchable, searchCol, columnSearchType));
                }

                for (int i = 0; i < this.ColumnsName.Length; i++)
                {
                    if (!string.IsNullOrEmpty(request.Params["iSortCol_" + i]))
                    {
                        int sortCol = int.Parse(request.Params["iSortCol_" + i]);
                        string sortDir = request.Params["sSortDir_" + i];
                        this.SortColumns.Add(new SortColumn(sortCol, sortDir));
                    }
                }

            }
        }

        private IList<ColumnInfo> _columns = null;
        private IList<SortColumn> _sortColumn = null;

        /// <summary>
        /// Display start point 
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Number of records to display
        /// </summary>
        public int Lenght { get; set; }
        
        
        /// <summary>
        /// Total number of records in the table
        /// </summary>
        public int Total { get; set; }
        
        /// <summary>
        /// Total number of filtered records 
        /// </summary>
        public int FilteredRecords { get; set; }
        
        /// <summary>
        /// Global search field 
        /// </summary>
        public string Search { get; set; }
        
        /// <summary>
        /// string array of column names 
        /// </summary>
        public string[] ColumnsName { get; set; }
        
        /// <summary>
        /// ColumnInfo list 
        /// </summary>
        public IList<ColumnInfo> Columns 
        { 
            get { 
                
                if(_columns == null) {
                    _columns = new List<ColumnInfo>();
                }

                return this._columns;
            }

            set { _columns = value; } 
        }

        /// <summary>
        /// SortColumn list 
        /// </summary>
        public IList<SortColumn> SortColumns
        {
            get
            {

                if (_sortColumn == null)
                {
                    _sortColumn = new List<SortColumn>();
                }

                return this._sortColumn;
            }

            set { _sortColumn = value; }
        }
    }

    /// <summary>
    /// Represents a FilterInfo column
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// Creates a ColumnInfo instance
        /// </summary>
        public ColumnInfo() { }
        
        /// <summary>
        /// Creates a ColumnInfo instance with the specified information.
        /// </summary>
        /// <param name="header">The column header</param>
        /// <param name="name">The column name</param>
        /// <param name="searchable">Signals if the column is searchable</param>
        /// <param name="search">The search value</param>
        public ColumnInfo(string header, string name, bool searchable, string search)
            : this(header, name, searchable, search, FilterInfo.ColumnSearchType.LIKE)
        {
            
        }

        /// <summary>
        /// Creates a ColumnInfo instance with the specified information.
        /// </summary>
        /// <param name="header">The column header</param>
        /// <param name="name">The column name</param>
        /// <param name="searchable">Signals if the column is searchable</param>
        /// <param name="search">The search value</param>
        /// <param name="searchType">The type of search</param>
        public ColumnInfo(string header, string name, bool searchable, string search, FilterInfo.ColumnSearchType searchType)
        {
            this.Header = header;
            this.Name = name;
            this.Searchable = searchable;
            this.Search = search;
            this.SearchType = searchType;
        }

        /// <summary>
        /// The column header
        /// </summary>
        public string Header { get; set; }
        
        /// <summary>
        /// The column name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Signals if the column is searchable
        /// </summary>
        public bool Searchable { get; set; }
        
        /// <summary>
        /// The search value
        /// </summary>
        public string Search { get; set; }
        
        /// <summary>
        /// The type of search
        /// </summary>
        public FilterInfo.ColumnSearchType SearchType { get; set; }
    }

    /// <summary>
    /// Represents a FilterInfo sortcolumn
    /// </summary>
    public class SortColumn
    {
        /// <summary>
        /// Creates a SortColumn instance
        /// </summary>
        public SortColumn() { }
        
        /// <summary>
        /// Creates a SortColumn instance with the specified info
        /// </summary>
        /// <param name="sortCol">The sort column index</param>
        /// <param name="sortDir">The sort column direction</param>
        public SortColumn(int sortCol, string sortDir)
        {
            this.SortCol = sortCol;
            this.SortDir = sortDir;
        }

        /// <summary>
        /// The sort column index
        /// </summary>
        public int SortCol { get; set; }
        
        /// <summary>
        /// The sort column direction
        /// </summary>
        public string SortDir { get; set; }
    }
}
