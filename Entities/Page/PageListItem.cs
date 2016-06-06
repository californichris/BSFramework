
namespace BS.Common.Entities.Page
{
    /// <summary>
    /// This Class is used for transferring PageListItem data from the data layer to the ui.
    /// Represents an HTML option.
    /// </summary>
    public class PageListItem
    {
        /// <summary>
        /// The Item unique identifier
        /// </summary>
        public string ItemId { get; set; }
        
        /// <summary>
        /// The name of the field that contains this item
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The value of the item
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// The text of the item.
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Short text of the item
        /// </summary>
        public string ShortText { get; set; }
        
        /// <summary>
        /// Signals if the item will be enable
        /// </summary>
        public string Enable { get; set; }
        
        /// <summary>
        /// Signals if the item will be selected
        /// </summary>
        public string Selected { get; set; }
        
        /// <summary>
        /// Last user to update the record
        /// </summary>
        public string UpdatedBy { get; set; }
        
        /// <summary>
        /// Last time the record was updated
        /// </summary>
        public string UpdatedDate { get; set; }
    }
}