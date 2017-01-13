using System;

namespace BS.Common.Entities
{
    /// <summary>
    /// Represents a transaction that will be executed in a datasource.
    /// </summary>
    public class TransOperation
    {
        /// <summary>
        /// The supported operation types
        /// </summary>
        public enum OperType
        {
            /// <summary>
            /// Save type
            /// </summary>
            Save,
            /// <summary>
            /// Update type
            /// </summary>
            Update,
            
            /// <summary>
            /// Delete type
            /// </summary>
            Delete,
            
            /// <summary>
            /// Delete all records thar meet the criteria
            /// </summary>
            DeleteEntities
        };

        /// <summary>
        /// Specifies the transaction operation type
        /// </summary>
        public OperType OperationType { get; set; }

        /// <summary>
        /// The entity that will be used in the operation
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Signal if the previous ids will be bind in the entity transacion operation
        /// </summary>
        public bool BindIds { get; set; }

        /// <summary>
        /// Creates an empty TransOperation instances
        /// </summary>
        public TransOperation()
        { 
        
        }


        /// <summary>
        /// Creates a TransOperation instances with the specified operType and entity
        /// </summary>
        /// <param name="operType">The transaction operation type</param>
        /// <param name="entity">The entity that will be used in the operation</param>
        public TransOperation(string operType, Entity entity)
        {
            if(operType.Equals("Save",StringComparison.CurrentCultureIgnoreCase)) {
                this.OperationType = OperType.Save;
            }
            else if (operType.Equals("Update", StringComparison.CurrentCultureIgnoreCase))
            {
                this.OperationType = OperType.Update;
            }
            else if (operType.Equals("Delete", StringComparison.CurrentCultureIgnoreCase))
            {
                this.OperationType = OperType.Delete;
            }
            else if (operType.Equals("DeleteEntities", StringComparison.CurrentCultureIgnoreCase))
            {
                this.OperationType = OperType.DeleteEntities;
            }
            else
            {
                throw new ArgumentException("Invalid operation type " + operType + ".");
            }

            this.Entity = entity;
            this.BindIds = true; //TODO: add logic to take this value from the ui, for now default to true
        }

    }
}
