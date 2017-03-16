using System.Collections.Generic;
using System.Web.Script.Serialization;
using BS.Common.Entities;
using BS.Common.Entities.Page;

namespace BS.Common.Utils
{
    /// <summary>
    /// This class consists of static utility methods to help with entity objects.
    /// </summary>
    public static class EntityUtils
    {
        /// <summary>
        /// Creates an entity based on the specified page and sets the entity fields.
        /// </summary>
        /// <param name="page">The page configuration</param>
        /// <returns>an entity with the proper fields</returns>
        public static Entity CreateEntity(Page page)
        {
            return CreateEntity(page, false);
        }

        /// <summary>
        /// Creates an entity based on the specified page and sets the entity fields.
        /// </summary>
        /// <param name="page">The page configuration</param>
        /// <param name="export">a flag to determine if the entity will contain not exported fields</param>
        /// <returns>an entity with the proper fields</returns>
        public static Entity CreateEntity(Page page, bool export)
        {
            Entity entity = new Entity(page.TableName);
            SetEntityFields(page, entity, export);
            return entity;
        }

        /// <summary>
        /// Populates the entity field list
        /// </summary>
        /// <param name="page">The page configuration</param>
        /// <param name="entity">The Entity to be populated</param>
        /// <param name="export">a flag to determine if the entity will contain not exported fields</param>
        public static void SetEntityFields(Page page, Entity entity, bool export)
        {
            int join = 1;

            foreach (PageTab tab in page.Tabs)
            {
                foreach (PageField field in tab.Fields)
                {
                    // if we are doing an export and the field is not exportable we don't need to retrieved the field
                    // so go to next field
                    if (export && field.Exportable != "True" && !IsForeignKey(field, page)) continue;

                    Field f = new Field(field.FieldName);

                    f.Id = field.IsId == "True";
                    f.Insertable = field.Insertable == "True";
                    f.Updatable = field.Updatable == "True";
                    f.DBName = field.DBFieldName;

                    if (IsForeignKey(field, page)) 
                    {
                        Entity joinInfo = GetJoinInfoEntity(field);
                        string[] joinFields = joinInfo.GetProperty("JoinFields").Split(',');
                        //TODO: Add logic to wrap fieldname with the proper character (sql [], oracle "", etc);
                        //f.ForeignKey = new ForeignKeyInfo(GetJoinType(joinInfo), joinInfo.GetProperty("TableName"), "[" + joinInfo.GetProperty("JoinField") + "] " + joinInfo.GetProperty("ExtraJoinDetails").Replace("#", join.ToString()), joinFields);                        
                        f.ForeignKey = new ForeignKeyInfo(GetJoinType(joinInfo), joinInfo.GetProperty("TableName"), joinInfo.GetProperty("JoinField") + " " + joinInfo.GetProperty("ExtraJoinDetails").Replace("#", join.ToString()), joinFields);                        
                        
                        foreach (string alias in joinFields)
                        {
                            entity.SetField(new Field(GetAliasName(alias), false, false));
                        }
                                                
                        join++;                    
                    }

                    f.DataType = GetFieldDataType(field);

                    entity.SetField(f);
                }
            }
        }

        /// <summary>
        /// Returns the field alias name. If the configured alias contains a "AS" it will return only the column name.
        /// </summary>
        /// <param name="alias">The configured alias name</param>
        /// <returns>The field alias name</returns>
        public static string GetAliasName(string alias)
        {
            string aliasName = alias;
            int asIndex = alias.ToUpper().IndexOf(" AS ");
            if (asIndex > -1)
            {
                aliasName = alias.Substring(asIndex + 4, alias.Length - (asIndex + 4)).Trim();
            }

            return aliasName;
        }

        /// <summary>
        /// It will determine the JoinType that will be performe for this field.
        /// </summary>
        /// <param name="joinInfo">The entity representing the join configuration</param>
        /// <returns>The join type</returns>
        public static Field.FKType GetJoinType(Entity joinInfo)
        {
            if (string.IsNullOrEmpty(joinInfo.GetProperty("JoinType")) || joinInfo.GetProperty("JoinType") == "LEFT") return Field.FKType.Left; //Default is Left
            else if (joinInfo.GetProperty("JoinType") == "INNER") return Field.FKType.Inner;
            else return Field.FKType.Right;
        }

        /// <summary>
        /// Creates an Entity representing the join configuration by deserializing the json configured value.
        /// </summary>
        /// <param name="field">The page field</param>
        /// <returns>The entity representing the join config.</returns>
        public static Entity GetJoinInfoEntity(PageField field)
        {
            Entity joinInfo = new Entity();
            Dictionary<string, string> props = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(field.JoinInfo);
            joinInfo.SetProperties(props);

            return joinInfo;
        }

        /// <summary>
        /// Determines the field data type of a configured page field.
        /// </summary>
        /// <param name="field">The page configured field</param>
        /// <returns>The field data type</returns>
        public static Field.DBType GetFieldDataType(PageField field)
        {
            if (field.Type.Contains("time"))
            {
                return Field.DBType.DateTime;
            }
            else if (field.Type.Contains("date"))
            {
                return Field.DBType.Date;
            }
            else if (field.Type == "int" || field.Type == "smallint")
            {
                return Field.DBType.Integer;
            }
            else if (field.Type == "float" || field.Type == "decimal" || field.Type == "money")
            {
                return Field.DBType.Decimal;
            }
            else if (field.Type.Contains("bit"))
            {
                return Field.DBType.Bit;
            }
            if (field.Type == "varbinary")
            {
                return Field.DBType.Varbinary;
            }
            if (field.Type == "encrypt")
            {
                return Field.DBType.Encrypt;
            }
            return Field.DBType.Varchar;
        }

        /// <summary>
        /// Determines if a field contains join information
        /// </summary>
        /// <param name="field">The configured field.</param>
        /// <param name="page">The page config that contains the field.</param>
        /// <returns>True if the fields contains join info false otherwise.</returns>
        public static bool IsForeignKey(PageField field, Page page) {
            return !string.IsNullOrEmpty(field.JoinInfo);
        }

        public static IList<Entity> DeserializeEntityList(string json)
        {
            return DeserializeEntityList(null, json);
        }

        public static IList<Entity> DeserializeEntityList(Entity entity, string json)
        {
            EntityConverter converter = (entity == null) ? new EntityConverter() : new EntityConverter(entity);

            IList<Entity> list = new List<Entity>();
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            ser.RegisterConverters(new JavaScriptConverter[] { converter });
            list = (List<Entity>)ser.Deserialize(json, typeof(IList<Entity>));

            return list;
        }

        public static string SerializeEntityList(IList<Entity> list)
        {
            EntityConverter converter = new EntityConverter();
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            ser.RegisterConverters(new JavaScriptConverter[] { converter });

            string data = ser.Serialize(list);

            return "[" + data + "]";
        }

    }
}
