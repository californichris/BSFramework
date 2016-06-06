
namespace BS.Common.Entities.Page
{
    /// <summary>
    /// Represents a grid column on the screen
    /// </summary>
    public class PageGridColumn
    {
        /// <summary>
        /// Unique identifier of the column
        /// </summary>        
        public string ColumnId { get; set; }

        /// <summary>
        /// The field if assosiated with this column
        /// </summary> 
        public string FieldId { get; set; }

        /// <summary>
        /// The page id containing this column
        /// </summary> 
        public string PageId { get; set; }

        /// <summary>
        /// The name of the column
        /// </summary> 
        public string ColumnName { get; set; }

        /// <summary>
        /// The label of the column on the screen, the header
        /// </summary> 
        public string ColumnLabel { get; set; }

        /// <summary>
        /// The order of the column on the grid
        /// </summary> 
        public string ColumnOrder { get; set; }

        /// <summary>
        /// signals if the column will be visible
        /// </summary> 
        public string Visible { get; set; }

        /// <summary>
        /// signals if the column will be searchable
        /// </summary> 
        public string Searchable { get; set; }

        /// <summary>
        /// the with of the columns in pixels, default to 0
        /// </summary> 
        public string Width { get; set; }

        /// <summary>
        /// Last user to update the column
        /// </summary> 
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Last updated date 
        /// </summary> 
        public string UpdatedDate { get; set; }     
    }
}