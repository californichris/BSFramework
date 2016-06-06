using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace BS.Common.Entities
{
    /// <summary>
    /// Provides custom serialization funtionality for Entity types.  
    /// </summary>
    public class EntityConverter : JavaScriptConverter
    {
        private Entity _entity = new Entity();
        
        /// <summary>
        /// Creates a EntityConverter instance.
        /// </summary>
        public EntityConverter()
        {
        }

        /// <summary>
        /// Creates a EntityConverter instance with the specified entity type.
        /// </summary>
        /// <param name="entity">The entity type</param>
        public EntityConverter(Entity entity)
        {
            this._entity = entity;
        }

        /// <summary>
        ///  Gets a collection of the supported types.
        /// </summary>
        public override IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(Entity); }
        }

        /// <summary>
        /// Builds a dictionary of name/value pairs from the specified object(Entity).
        /// </summary>
        /// <param name="obj">The entity to serialize.</param>
        /// <param name="serializer">The object that is responsible for the serialization.</param>
        /// <returns>An object that contains key/value pairs that represent the entity data.</returns>
        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            if (obj is Entity)
            {
                return (IDictionary<string, object>) ((Entity)obj).GetProperties();
            }

            return null;
        }

        /// <summary>
        /// Converts the provided dictionary into an entity object.
        /// </summary>
        /// <param name="dictionary">A System.Collections.Generic.IDictionary{TKey,TValue} instance of property data stored as name/value pairs.</param>
        /// <param name="type">The type of the resulting object.</param>
        /// <param name="serializer">The JavaScriptSerializer instance.</param>
        /// <returns>The deserialized entity.</returns>
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            Entity entity = (Entity) Activator.CreateInstance(_entity.GetType());
            entity.SetFields(this._entity.GetFields());

            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                entity.SetProperty(pair.Key, pair.Value.ToString());
            }
            

            return entity;
        }
    }
}
