using System.Collections.Generic;

namespace BS.Common.Entities.Page
{
    /// <summary>
    /// Represents a dialog tab on the screen
    /// </summary>
    public class PageTab
    {
        private List<PageField> _fields = null;

        /// <summary>
        /// The Page Tab unique identifier
        /// </summary>
        public string TabId { get; set; }

        /// <summary>
        /// The Page id containing this tab
        /// </summary>
        public string PageId { get; set; }
        
        /// <summary>
        /// The Page Tab Name
        /// </summary>
        public string TabName { get; set; }

        /// <summary>
        /// Not being used TODO: delete this property
        /// </summary>
        public string URL { get; set; }
        
        /// <summary>
        /// The Tab Order on the screen from left to rigth
        /// </summary>
        public string TabOrder { get; set; }
        
        /// <summary>
        /// The number of columns in the tab
        /// </summary>
        public string Cols { get; set; }

        /// <summary>
        /// Last user to update the page tab
        /// </summary>
        public string UpdatedBy { get; set; }
 
        /// <summary>
        /// Last updated date
        /// </summary>
        public string UpdatedDate { get; set; }

        /// <summary>
        /// The Tab fields
        /// </summary>
        public List<PageField> Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = new List<PageField>();
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