using System.Collections.Generic;

namespace BS.Common.Entities.Page
{
    /// <summary>
    /// This Class is used for transferring Page data from the data layer to the ui.
    /// Contains the necessary information to construct a page.
    /// </summary>
    public class Page
    {
        private List<PageTab> _tabs = null;
        private List<PageGridColumn> _gridFields = null;

        /// <summary>
        /// The page app unique identifier 
        /// </summary>
        public string PageAppId { get; set; }

        /// <summary>
        /// The page unique identifier 
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        /// The Name of the page 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Title of the page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The TableName that is associated with this page. 
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Last user to update the page 
        /// </summary>
        public string UpdatedBy { get; set; }
        
        /// <summary>
        /// Last updated date 
        /// </summary>
        public string UpdatedDate { get; set; }

        /// <summary>
        /// Connection Name
        /// </summary>
        public string ConnName { get; set; }

        /// <summary>
        /// The page grid fields
        /// </summary>
        public List<PageGridColumn> GridFields
        {
            get
            {
                if (_gridFields == null)
                {
                    _gridFields = new List<PageGridColumn>();
                }
                return _gridFields;
            }

            set
            {
                _gridFields = value;
            }
        }
        
        /// <summary>
        /// The page tabs 
        /// </summary>
        public List<PageTab> Tabs
        {
            get
            {
                if (_tabs == null)
                {
                    _tabs = new List<PageTab>();
                }
                return _tabs;
            }

            set
            {
                _tabs = value;
            }
        }

        /// <summary>
        /// The page filter
        /// </summary>
        public PageFilter Filter { get; set; }
    }
}