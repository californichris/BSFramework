
namespace BS.Common.Entities.Page
{
    /// <summary>
    /// Represents a page filter on the screen
    /// </summary>
    public class PageFilterField
    {
        /// <summary>
        /// The unique identifier of the filter field
        /// </summary>
        public string FilterFieldId { get; set; }

        /// <summary>
        /// The filter id containing this field
        /// </summary>
        public string FilterId { get; set; }

        /// <summary>
        /// The field identifier
        /// </summary>
        public string FieldId { get; set; }

        /// <summary>
        /// the order in the filter
        /// </summary>
        public string FilterOrder { get; set; }

        /// <summary>
        /// Last user to update the page filter field
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Last updated date
        /// </summary>
        public string UpdatedDate { get; set; }
    }
}
