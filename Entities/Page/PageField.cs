
namespace BS.Common.Entities.Page
{
    /// <summary>
    /// Represents a field on the screen in other words an HTML element.
    /// </summary>
    public class PageField
    {
        /// <summary>
        /// The unique identifier of the field
        /// </summary>
        public string FieldId { get; set; }

        /// <summary>
        /// The Tab id containing this tab
        /// </summary>
        public string TabId { get; set; }

        /// <summary>
        /// the name  of the element on the screen
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The label that will be display on the screen
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The data base type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// signals if a field is required
        /// </summary>
        public string Required { get; set; }

        /// <summary>
        /// contains the information to create select type html elements (selectmenu, combobox, multiselects), the information
        /// is in a JSON format.
        /// </summary>
        public string DropDownInfo { get; set; }

        /// <summary>
        /// signals if the field will be exportable
        /// </summary>
        public string Exportable { get; set; }

        /// <summary>
        /// the order in the tab
        /// </summary>
        public string FieldOrder { get; set; }

        /// <summary>
        /// The type of control that will be created on the screen
        /// </summary>
        public string ControlType { get; set; }

        /// <summary>
        /// signals if the field is the id
        /// </summary>
        public string IsId { get; set; }

        /// <summary>
        /// Last user to update the field
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Last updated date
        /// </summary>
        public string UpdatedDate { get; set; }

        /// <summary>
        /// contains the join information with other tables on a JSON format.
        /// </summary>
        public string JoinInfo { get; set; }

        /// <summary>
        /// the database column name
        /// </summary>
        public string DBFieldName { get; set; }

        /// <summary>
        /// signals if he field will be included in insert clauses
        /// </summary>
        public string Insertable { get; set; }

        /// <summary>
        /// signals if the field wil be included in update clauses
        /// </summary>
        public string Updatable { get; set; }

        /// <summary>
        /// if the field will be displayed in the page grid, this property contains the information of the column
        /// </summary>
        public PageGridColumn ColumnInfo { get; set; }

        /// <summary>
        /// contains control properties in a JSON format 
        /// </summary>
        public string ControlProps { get; set; }
    }
}