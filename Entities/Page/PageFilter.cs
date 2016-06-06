using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BS.Common.Entities.Page
{
    public class PageFilter
    {
        private List<PageFilterField> _fields = null;

        /// <summary>
        /// The unique identifier of the filter
        /// </summary>
        public string FilterId { get; set; }

        /// <summary>
        /// The page id containing this filter
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        /// the text that will be display in the filter header
        /// </summary>
        public string FilterText { get; set; }

        /// <summary>
        /// The number of columns in the filter
        /// </summary>
        public string FilterCols { get; set; }

        /// <summary>
        /// signals if the filter will display the clear button
        /// </summary>
        public string ShowClear { get; set; }

        /// <summary>
        /// contains filter properties in a JSON format 
        /// </summary>
        public string FilterProps { get; set; }

        /// <summary>
        /// Last user to update the page filter
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Last updated date
        /// </summary>
        public string UpdatedDate { get; set; }

        /// <summary>
        /// The Tab fields
        /// </summary>
        public List<PageFilterField> Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = new List<PageFilterField>();
                }
                return _fields;
            }

            set
            {
                _fields = value;
            }
        }
    }
}
