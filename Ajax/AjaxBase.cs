using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using BS.Common.Utils;
using System.Text;
using BS.Common.Entities;
using System.Web;
using BS.Common.Entities.Page;
using System.Configuration;
using System.IO;

namespace BS.Common.Ajax
{
    /// <summary>
    /// This is the super class for all ajax handler classes.
    /// Contains a set of common methods. This class can't be instatiated.
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public abstract class AjaxBase
    {
        /// <summary>
        /// Creates a json success response
        /// </summary>
        /// <returns>The success JSON response</returns>
        protected virtual string SuccessResponse()
        {
            return "{\"ErrorMsg\" : \"Success\"}";
        }

        /// <summary>
        /// Creates a json success response with the id of the specified entity
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <returns>The success JSON response</returns>
        protected virtual string SuccessResponse(Entity entity)
        {
            string id = "";
            if (entity != null && entity.GetEntityId() != null) id = entity.GetEntityId();
            return "{\"ErrorMsg\" : \"Success\", \"Id\" : \"" + id + "\"}";
        }

        /// <summary>
        /// Creates a json success response with the id of the specified entity
        /// </summary>
        /// <param name="id">the new id</param>
        /// <returns>The success JSON response</returns>
        protected virtual string SuccessResponse(string id)
        {
            return "{\"ErrorMsg\" : \"Success\", \"Id\" : \"" + id + "\"}";
        }

        /// <summary>
        /// Creates a json error response
        /// </summary>
        /// <param name="msg">Error message</param>
        /// <returns>The error JSON response</returns>
        protected virtual string ErrorResponse(string msg)
        {
            IDictionary<string, string> error = new Dictionary<string, string>();
            error.Add("ErrorMsg", msg);
            JavaScriptSerializer ser = new JavaScriptSerializer();

            return ser.Serialize(error);
        }

        /// <summary>
        /// Creates a json error response using the exception argument
        /// </summary>
        /// <param name="e">a System.Exception</param>
        /// <returns>The error JSON response</returns>
        protected virtual string ErrorResponse(Exception e)
        {
            return ErrorResponse(e.Message);
        }

        /// <summary>
        /// Creates a json used by DataTables to populate an html table.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="list">generic list of objects</param>
        /// <returns>The JSON response</returns>
        protected virtual string ListResponse<T>(IList<T> list)
        {
            return "{\"iTotalRecords\":" + list.Count + ",\"iTotalDisplayRecords\":" + list.Count + ",\"aaData\":" + SerializeList(list) + "}";        
        }

        /// <summary>
        /// Converts a list of objects to a JSON array string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="list">The list to serialize</param>
        /// <returns>The JSON array</returns>
        protected virtual string SerializeList<T>(IList<T> list)
        {
            Type typeParameterType = typeof(T);
            if (typeParameterType.Equals(typeof(Entity)))
            {
                return SerializeEntityList((IList<Entity>)list);
            }
            else
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                ser.MaxJsonLength = int.MaxValue;

                return ser.Serialize(list);
            } 
        }

        /// <summary>
        /// Creates a JSON used by DataTables to populate an html table.
        /// </summary>
        /// <param name="list">list of entity objects</param>
        /// <returns>The JSON response</returns>
        protected virtual string CreateEntityListResponse(IList<Entity> list)
        {
            return CreateEntityListResponse(list, null);
        }

        /// <summary>
        /// Creates a json used by DataTables to populate an html table.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="list">generic list of objects</param>
        /// <param name="filter">A filter object that contains the total number of records and filter number of records.</param>
        /// <returns>The JSON response</returns>
        protected virtual string ListResponse<T>(IList<T> list, FilterInfo filter)
        {
            return "{\"iTotalRecords\":" + filter.Total + ",\"iTotalDisplayRecords\":" + filter.FilteredRecords + ",\"aaData\":" + SerializeList(list) + "}";
        }

        /// <summary>
        /// Creates a json used by DataTables to populate an html table.
        /// </summary>
        /// <param name="list">generic list of objects</param>
        /// <param name="filter">A filter object that contains the total number of records and filter number of records.</param>
        /// <returns>The JSON response</returns>
        protected virtual string CreateEntityListResponse(IList<Entity> list, FilterInfo filter)
        {            
            int total = list.Count;
            int filtered = list.Count;

            if (filter != null)
            {
                total = filter.Total;
                filtered = filter.FilteredRecords;
            }

            return "{\"iTotalRecords\":" + total + ",\"iTotalDisplayRecords\":" + filtered + ",\"aaData\":" + SerializeEntityList(list) + "}";
        }

        /// <summary>
        /// Converts a list of entities to a JSON array string. 
        /// </summary>
        /// <param name="list">The list of entities to be converted</param>
        /// <returns>The JSON array</returns>
        protected virtual string SerializeEntityList(IList<Entity> list)
        {
            //TODO: updated this method to used the EntityConverter
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
        /// Determines if the request is an export to csv
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <returns>True if the request param csv is true or false otherwise</returns>
        protected virtual bool IsCSV(HttpRequest request)
        {
            return !string.IsNullOrEmpty(request.Params["csv"]) && bool.Parse(request.Params["csv"]) ? true : false;
        }

        /// <summary>
        /// Creates an entity from the specified page and sets the entity date from the request if any.
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="page">The page configuration</param>
        /// <param name="csv">signals if the entity will include exportable fields</param>
        /// <returns>The entity</returns>
        protected virtual Entity CreateEntity(HttpRequest request, Page page, bool csv)
        {
            Entity entity = EntityUtils.CreateEntity(page, csv);
            string json = request.Params[PageInfo.EntityParam];
            LoggerHelper.Debug("entity = " + json);

            if (!String.IsNullOrEmpty(json))
            {
                Dictionary<string, string> props = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(json);
                entity.SetProperties(props);
            }

            return entity;
        }

        /// <summary>
        /// Creates an entity from the specified page and sets the entity date from the request if any.
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="page">The page configuration</param>
        /// <returns>The entity</returns>
        protected virtual Entity CreateEntity(HttpRequest request, Page page)
        {
            return CreateEntity(request, page, false);
        }

        /// <summary>
        /// Converts a list of entities to csv
        /// </summary>
        /// <param name="list">list of entities</param>
        /// <param name="allColumns">signal if all exportable columns will be included in the csv</param>
        /// <param name="page">The page configuration</param>
        /// <returns>a csv response</returns>
        public virtual string EntityListToCSV(IList<Entity> list, bool allColumns, Page page)
        {
            string separator = GetListSeparator();

            StringBuilder response = new StringBuilder();
            response.Append(CreateHeader(page, allColumns, separator));

            IList<PageField> fields = GetExportableFields(page);
            foreach (Entity ent in list)
            {
                response.Append(CreateRow(ent, page, fields, allColumns, separator));
            }

            return response.ToString();
        }

        /// <summary>
        /// Creates a csv row
        /// </summary>
        /// <param name="ent">The entity</param>
        /// <param name="page">The page configuration</param>
        /// <param name="fields">The list of exportable fields</param>
        /// <param name="allColumns">signal if all exportable columns will be included in the csv</param>
        /// <param name="separator">The list separator</param>
        /// <returns>The csv row</returns>
        protected virtual string CreateRow(Entity ent, Page page, IList<PageField> fields, bool allColumns, string separator)
        {
            StringBuilder response = new StringBuilder();

            if (allColumns)
            {
                foreach (PageField field in fields)
                {
                    string fieldName = field.FieldName;
                    if (!string.IsNullOrEmpty(field.DropDownInfo) && !string.IsNullOrEmpty(field.JoinInfo))
                    {
                        Entity joinInfo = EntityUtils.GetJoinInfoEntity(field);
                        string[] joinFields = joinInfo.GetProperty("JoinFields").Split(',');

                        foreach (string alias in joinFields)
                        {
                            response.Append("\"").Append(ent.GetProperty(EntityUtils.GetAliasName(alias)).Replace("\"", "\"\"")).Append("\"").Append(separator);
                        }
                    }
                    else
                    {
                        response.Append("\"").Append(ent.GetProperty(fieldName).Replace("\"", "\"\"")).Append("\"").Append(separator);
                    }
                }
            }
            else
            {
                foreach (PageGridColumn col in page.GridFields)
                {
                    response.Append("\"").Append(ent.GetProperty(col.ColumnName).Replace("\"", "\"\"")).Append("\"").Append(separator);
                }
            }

            response.Remove(response.Length - separator.Length, separator.Length);
            response.Append(System.Environment.NewLine);

            return response.ToString();
        }

        /// <summary>
        /// Creates the csv header
        /// </summary>
        /// <param name="page">The page configuration</param>
        /// <param name="allColumns">signal if all exportable columns will be included in the csv</param>
        /// <param name="separator">The list separator</param>
        /// <returns>The csv header</returns>
        protected virtual string CreateHeader(Page page, bool allColumns, string separator)
        {
            StringBuilder response = new StringBuilder();

            if (allColumns)
            {
                IList<PageField> fields = GetExportableFields(page);
                foreach (PageField field in fields)
                {
                    string fieldName = field.FieldName;
                    if (!string.IsNullOrEmpty(field.DropDownInfo) && !string.IsNullOrEmpty(field.JoinInfo))
                    {
                        Entity joinInfo = EntityUtils.GetJoinInfoEntity(field);
                        string[] joinFields = joinInfo.GetProperty("JoinFields").Split(',');

                        foreach (string alias in joinFields)
                        {
                            response.Append("\"").Append(EntityUtils.GetAliasName(alias).Replace("\"", "\"\"")).Append("\"").Append(separator);
                        }
                    }
                    else
                    {
                        response.Append("\"").Append(field.Label.Replace("\"", "\"\"")).Append("\"").Append(separator);
                    }
                }
            }
            else
            {
                foreach (PageGridColumn col in page.GridFields)
                {
                    response.Append("\"").Append(col.ColumnLabel).Append("\"").Append(separator);
                }
            }

            response.Remove(response.Length - separator.Length, separator.Length);
            response.Append(System.Environment.NewLine);

            return response.ToString();
        }

        /// <summary>
        /// Determines which fielda are exportable
        /// </summary>
        /// <param name="page">The page configuration</param>
        /// <returns>The list of exportable fields</returns>
        protected virtual IList<PageField> GetExportableFields(Page page)
        {
            IList<PageField> fields = new List<PageField>();

            foreach (PageTab tab in page.Tabs)
            {
                foreach (PageField field in tab.Fields)
                {
                    if (field.Exportable == "True")
                        fields.Add(field);
                }
            }

            return fields;
        }

        /// <summary>
        /// Determines the list separator that will be used in a csv response.
        /// By default it will used the system list separator property but can 
        /// be overridden by the ListSeparator application setting.
        /// 
        /// </summary>
        /// <returns>The list separator</returns>
        protected virtual string GetListSeparator()
        {
            string separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator; //Default
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ListSeparator"]))
            {
                separator = ConfigurationManager.AppSettings["ListSeparator"];
            }

            return separator;
        }

        /// <summary>
        /// Gets the current user name
        /// </summary>
        /// <returns>The current user name</returns>
        protected virtual string GetCurrentUserName()
        {
            string userName = "";
            string[] name = {""};
            if (!string.IsNullOrEmpty(System.Web.HttpContext.Current.User.Identity.Name))
            {
                name = System.Web.HttpContext.Current.User.Identity.Name.Split('\\');
                userName = System.Web.HttpContext.Current.User.Identity.Name;
            }


            if (name.Length > 1)
            {
                userName = name[1];
            }
            
            return userName;
        }
    }
}