using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using BS.Common.Entities;
using BS.Common.Utils;

namespace BS.Common.Dao.File
{
    /// <summary>
    /// Base class for all File implementations
    /// </summary>
    public abstract class BaseFileDAO : BaseDAO
    {
        /// <summary>
        /// The Offline folder name
        /// </summary>
        public static readonly string OfflineDataFolderName = string.IsNullOrEmpty(ConfigurationManager.AppSettings["OfflineFolderName"]) ? "Offline" : ConfigurationManager.AppSettings["OfflineFolderName"];

        /// <summary>
        /// Determines the file name depending on the specified entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The file name</returns>
        protected virtual string GetFileName(Entity entity)
        {
            string fileName = entity.GetTableName();
            if (entity.GetTableName().Equals("Cities") && !string.IsNullOrEmpty(entity.GetProperty("CountryCode")))
            {
                fileName = "Cities/" + fileName + entity.GetProperty("CountryCode");
            }

            fileName += ".txt";
            LoggerHelper.Debug("FileName: " + fileName);

            return fileName;
        }

        /// <summary>
        /// Loads the data related with the specified entity from a file.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The entity data</returns>
        protected virtual string LoadFile(Entity entity)
        {
            return LoadFile(GetFileName(entity));
        }

        /// <summary>
        /// Loads the data from the specified file if the file is not found it creates an empty file.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The loaded data</returns>
        protected virtual string LoadFile(string fileName)
        {
            string json = "[]";
            string path = "";
            try
            {
                path = HttpContext.Current.Server.MapPath("~/" + OfflineDataFolderName + "/" + fileName);
                json = FileUtils.LoadFile(path);
            }
            catch (System.IO.FileNotFoundException e)
            {
                LoggerHelper.Error(e);
                //File not found creating and empty list.
                SaveToFile("[]", fileName);
            }

            return json;
        }

        /// <summary>
        /// Converts the JSON-formatted string to a list of entities using the specified entity type
        /// to convert each object.
        /// </summary>
        /// <param name="entity">The entity type</param>
        /// <param name="json">The JSON-formatted string</param>
        /// <returns>The list of entities</returns>
        protected virtual IList<Entity> DeserializeEntityList(Entity entity, string json)
        {
            EntityConverter converter = (entity == null) ? new EntityConverter() : new EntityConverter(entity);
            
            IList<Entity> list = new List<Entity>();
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            ser.RegisterConverters(new JavaScriptConverter[] { converter });
            list = (List<Entity>) ser.Deserialize(json, typeof(IList<Entity>));
            
            LoggerHelper.Debug("Done deserializing data.");
            
            return list;
        }

        /// <summary>
        /// Converts the JSON-formatted string to a list of entities
        /// </summary>
        /// <param name="json">The JSON-formatted string</param>
        /// <returns>The list of entities</returns>
        protected virtual IList<Entity> DeserializeEntityList(string json)
        {
            return DeserializeEntityList(null, json);
        }

        /// <summary>
        /// Converts the specified list of objects to a JSON-formated string and saves it into the
        /// specified file.
        /// </summary>
        /// <typeparam name="T">The type of object</typeparam>
        /// <param name="list">The list of objects</param>
        /// <param name="fileName">The file name</param>
        protected virtual void SaveToFile<T>(IList<T> list, string fileName)
        {
            Type typeParameterType = typeof(T);
            if (typeParameterType.Equals(typeof(Entity)))
            {
                SaveToFile(SerializeEntityList((IList<Entity>)list), fileName);
            }
            else
            {
                SaveToFile(SerializeList(list), fileName);
            }
        }

        /// <summary>
        /// Saves the specified data to the specified file.
        /// </summary>
        /// <param name="data">The data to be saved</param>
        /// <param name="fileName">The file name</param>
        protected virtual void SaveToFile(string data, string fileName)
        {
            string path = HttpContext.Current.Server.MapPath("~/" + OfflineDataFolderName + "/" + fileName);
            FileUtils.SaveToFile(data, path);
        }

        /// <summary>
        /// Converts the specified list of entities to a JSON-formated string.
        /// </summary>
        /// <param name="list">The entity list</param>
        /// <returns>The JSON-formated data</returns>
        protected virtual string SerializeEntityList(IList<Entity> list)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            StringBuilder aaData = new StringBuilder();
            
            foreach (Entity ent in list)
            {
                aaData.Append(ser.Serialize(ent.GetProperties())).Append(",");
            }

            if (aaData.Length > 0)
            {
                aaData.Remove(aaData.Length - 1, 1);
            }

            return "[" + aaData.ToString() + "]";
        }

        /// <summary>
        /// Converts the specified list of objects to a JSON-formated string.
        /// </summary>
        /// <typeparam name="T">The type of objects to convert</typeparam>
        /// <param name="list">The list of objects</param>
        /// <returns>The JSON-formated data</returns>
        protected virtual string SerializeList<T>(IList<T> list)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;

            return ser.Serialize(list);
        }

        /// <summary>
        /// Joins a existing list of entities with all the foreign tables defined
        /// in the Entity type
        /// </summary>
        /// <param name="entities">The list of entities</param>
        /// <param name="entity">The entity type</param>
        protected virtual void MergeData(List<Entity> entities, Entity entity)
        {
            List<Field> fields = entity.GetFields().FindAll(f => f.ForeignKey != null);
            foreach (Field field in fields)
            {
                ForeignKeyInfo fk = field.ForeignKey;
                string json = LoadFile(fk.TableName + ".txt");
                if (string.IsNullOrEmpty(json)) continue;
                IList<Entity> list = DeserializeEntityList(json);

                //TODO: implement RIGTH join
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    Entity ent = entities[i];

                    string joinfield = fk.JoinField.Replace("[", "").Replace("]", "").Trim();
                    Entity result = ((List<Entity>)list).Find(item => item.GetProperty(joinfield).Equals(ent.GetProperty(field.Name)));

                    if (result == null && fk.Type == Field.FKType.Left) //LEFT
                    {
                        foreach (string jf in fk.JoinFields)
                        {
                            string name = jf;
                            string value = "";

                            if (jf.IndexOf(" AS ") != -1)
                            {
                                string[] fieldAs = jf.Split(new string[] { " AS " }, StringSplitOptions.None);
                                if (fieldAs.Length == 2)
                                {
                                    name = fieldAs[1];
                                }
                            }

                            ent.SetProperty(name, value);
                        }
                    }
                    else if (result == null && fk.Type == Field.FKType.Inner) //INNER
                    {
                        entities.RemoveAt(i);
                    }
                    else
                    {
                        foreach (string jf in fk.JoinFields)
                        {
                            string name = jf;
                            string value = result.GetProperty(jf);

                            if (jf.IndexOf(" AS ") != -1)
                            {
                                string[] fieldAs = jf.Split(new string[] { " AS " }, StringSplitOptions.None);
                                if (fieldAs.Length == 2)
                                {
                                    name = fieldAs[1];
                                    value = result.GetProperty(fieldAs[0]);
                                }
                            }

                            ent.SetProperty(name, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Filters the specified entity list using the specified filter
        /// </summary>
        /// <param name="list">The list of entities</param>
        /// <param name="entity">The entity type</param>
        /// <param name="filter">The filter</param>
        /// <returns>The filtered list</returns>
        protected virtual List<Entity> FilterEntities(IList<Entity> list, Entity entity, FilterInfo filter)
        {
            List<Entity> filteredItems = (List<Entity>)list;

            //Starting quick filter
            if (!string.IsNullOrEmpty(filter.Search))
            {
                filteredItems = filteredItems.FindAll(ent => QuickFilter(ent, filter));
            }

            //Starting AND filtering
            List<ColumnInfo> cols = ((List<ColumnInfo>)filter.Columns).FindAll(col => col.Searchable && !string.IsNullOrEmpty(col.Search));

            //get fields that have joins and search for the column in the join fields
            if (cols.Count > 0)
            {
                foreach (ColumnInfo col in cols)
                {
                    string fieldName = GetFieldName(entity, col.Name);
                    if (col.Search.IndexOf("_RANGE_") == -1)
                    {
                        if (col.SearchType == FilterInfo.ColumnSearchType.LIKE)
                            filteredItems = filteredItems.FindAll(ent => ent.GetProperty(fieldName).IndexOf(col.Search, StringComparison.CurrentCultureIgnoreCase) >= 0);
                        else
                            filteredItems = filteredItems.FindAll(ent => ent.GetProperty(fieldName).Equals(col.Search));
                    }
                    else
                    {
                        //Filtering range
                        filteredItems = filteredItems.FindAll(ent => DateFilter(ent, col));
                    }
                }
            }

            return filteredItems;
        }

        /// <summary>
        /// Search the specified field name in all entity foreign fields and
        /// returns the field name if found otherwise returns the same fieldname 
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="fieldName">The field name</param>
        /// <returns>The field name</returns>
        protected virtual string GetFieldName(Entity entity, string fieldName)
        {
            List<Field> fields = entity.GetFields().FindAll(f => f.ForeignKey != null);
            foreach (Field field in fields)
            {
                int index = field.ForeignKey.JoinFields.IndexOf(fieldName);
                if (index != -1)
                {
                    return field.Name;
                }
            }

            return fieldName;
        }

        /// <summary>
        /// A predicate that defines a set of criteria and determines whether
        /// the specified entity meets those criteria defined in the filter.
        /// </summary>
        /// <param name="ent">The entity</param>
        /// <param name="filter">The filter info</param>
        /// <returns>True if the entity meets the criteria False otherwise</returns>
        protected virtual bool QuickFilter(Entity ent, FilterInfo filter)
        {
            bool found = false;
            List<ColumnInfo> cols = ((List<ColumnInfo>)filter.Columns).FindAll(col => col.Searchable);
            foreach (ColumnInfo col in cols)
            {
                if (!string.IsNullOrEmpty(ent.GetProperty(col.Name)))
                {
                    if (col.SearchType == FilterInfo.ColumnSearchType.LIKE)
                    {
                        found = ent.GetProperty(col.Name).IndexOf(filter.Search, StringComparison.CurrentCultureIgnoreCase) >= 0;
                    }
                    else
                    {
                        found = ent.GetProperty(col.Name).Equals(filter.Search, StringComparison.CurrentCultureIgnoreCase);
                    }
                }

                if (found) return found;
            }

            return found;
        }

        /// <summary>
        /// Represents the way to compares two entities
        /// </summary>
        /// <param name="ent1">The first entity to compare.</param>
        /// <param name="ent2">The second entity to compare.</param>
        /// <param name="filter">The filter info</param>
        /// <returns>A signed integer that indicates the relative values of x and y, as shown in the following table.Value Meaning Less than 0 x is less than y.0 x equals y.Greater than 0 x is greater than y.</returns>
        protected virtual int SortEntities(Entity ent1, Entity ent2, FilterInfo filter)
        {
            foreach (SortColumn sortCol in filter.SortColumns)
            {
                string property = filter.Columns[sortCol.SortCol].Name;
                int result = 0;
                if (sortCol.SortDir.Equals("ASC", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = ent1.GetProperty(property).CompareTo(ent2.GetProperty(property));

                }
                else
                {
                    result = ent2.GetProperty(property).CompareTo(ent1.GetProperty(property));
                }

                if (result != 0) return result;
            }
            return 0;
        }

        /// <summary>
        /// A predicate that defines a set of criteria and determines whether
        /// the specified entity meets those criteria.
        /// </summary>
        /// <param name="ent">The entity</param>
        /// <param name="col">The column info</param>
        /// <returns>True if the entity meets the criteria False otherwise</returns>
        protected virtual bool DateFilter(Entity ent, ColumnInfo col)
        {
            string[] rangeValues = col.Search.Split(new string[] { "_RANGE_" }, StringSplitOptions.None);

            if (!string.IsNullOrEmpty(rangeValues[0]) && !string.IsNullOrEmpty(rangeValues[1]))
            {
                DateTime from = new DateTime();
                DateTime to = new DateTime();
                DateTime value = new DateTime();

                if (DateTime.TryParse(rangeValues[0], out from) && DateTime.TryParse(rangeValues[1], out to) &&
                    DateTime.TryParse(ent.GetProperty(col.Name), out value))
                {
                    if (value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0)
                    {
                        return true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(rangeValues[0]))
            {
                DateTime from = new DateTime();
                DateTime value = new DateTime();

                if (DateTime.TryParse(rangeValues[0], out from) && DateTime.TryParse(ent.GetProperty(col.Name), out value))
                {
                    if (value.CompareTo(from) >= 0)
                    {
                        return true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(rangeValues[1]))
            {
                DateTime to = new DateTime();
                DateTime value = new DateTime();

                if (DateTime.TryParse(rangeValues[1], out to) && DateTime.TryParse(ent.GetProperty(col.Name), out value))
                {
                    if (value.CompareTo(to) <= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}