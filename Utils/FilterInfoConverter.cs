using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace BS.Common.Utils
{
    public class FilterInfoConverter : JavaScriptConverter
    {
        /// <summary>
        ///  Gets a collection of the supported types.
        /// </summary>
        public override IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(FilterInfo); }
        }

        /// <summary>
        /// Builds a dictionary of name/value pairs from the specified object(FilterInfo).
        /// </summary>
        /// <param name="obj">The entity to serialize.</param>
        /// <param name="serializer">The object that is responsible for the serialization.</param>
        /// <returns>An object that contains key/value pairs that represent the entity data.</returns>
        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {            
            return null;
        }

        /// <summary>
        /// Converts the provided dictionary into a FilterInfo object.
        /// </summary>
        /// <param name="dictionary">A System.Collections.Generic.IDictionary{TKey,TValue} instance of property data stored as name/value pairs.</param>
        /// <param name="type">The type of the resulting object.</param>
        /// <param name="serializer">The JavaScriptSerializer instance.</param>
        /// <returns>The deserialized entity.</returns>
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            if (type == typeof(FilterInfo))
            {
                FilterInfo filterInfo = new FilterInfo();
                filterInfo.Start = (int) dictionary["start"];
                filterInfo.Lenght = (int) dictionary["length"];

                foreach (KeyValuePair<string, object> pair in dictionary)
                {
                    if (pair.Key.Equals("columns"))
                    {
                        System.Collections.ArrayList array = (System.Collections.ArrayList)pair.Value;
                        foreach (Dictionary<string, object> item in array)
                        {
                            filterInfo.Columns.Add(createColumnInfo(item));
                        }
                    }
                    else if (pair.Key.Equals("order"))
                    {
                        System.Collections.ArrayList array = (System.Collections.ArrayList)pair.Value;
                        foreach (Dictionary<string, object> item in array)
                        {
                            filterInfo.SortColumns.Add(createSortColumn(item));
                        }
                    }
                    else if (pair.Key.Equals("search"))
                    {
                        IDictionary<string, object> search = (IDictionary<string, object>)pair.Value;
                        filterInfo.Search = (string) search["value"];
                    }
                }

                return filterInfo;
            }

            return null;
        }

        private ColumnInfo createColumnInfo(Dictionary<string, object> item)
        {
            ColumnInfo column = new ColumnInfo();
            column.Header = (string) item["name"];
            column.Name = (string) item["data"];

            IDictionary<string, object> search = (IDictionary<string, object>)item["search"];
            column.Search = (string) search["value"];
            
            column.Searchable = (bool) item["searchable"];
            column.SearchType = FilterInfo.ColumnSearchType.LIKE;

            if (item.ContainsKey("searchtype"))
            {
                string stype = (string) item["searchtype"];
                if (!string.IsNullOrEmpty(stype) && "equals".Equals(stype))
                {
                    column.SearchType = FilterInfo.ColumnSearchType.EQUALS;
                }
                else if (!string.IsNullOrEmpty(stype) && "null".Equals(stype))
                {
                    column.SearchType = FilterInfo.ColumnSearchType.NULL;
                }
            }

            return column;
        }

        private SortColumn createSortColumn(Dictionary<string, object> item)
        {
            SortColumn sortColumn = new SortColumn();
            sortColumn.SortCol = (int) item["column"];
            sortColumn.SortDir = (string) item["dir"];

            return sortColumn;
        }
    }
}
