using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BS.Common.Entities
{
    public class TransOperation
    {
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
            Delete
        };

        /// <summary>
        /// Specifies the transaction operation type
        /// </summary>
        public OperType OperationType { get; set; }

        /// <summary>
        /// The entity that will be used in the operation
        /// </summary>
        public Entity Entity { get; set; }

        public TransOperation()
        { 
        
        }

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
            else
            {
                throw new ArgumentException("Invalid operation type " + operType + ".");
            }

            this.Entity = entity;
        }

    }
}
