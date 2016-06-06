using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using BS.Common.Dao;
using BS.Common.Entities;
using BS.Common.Utils;
using System.Configuration;

namespace BS.Common.Ajax
{
    /// <summary>
    ///     This is the super class for all catalog ajax handler classes.
    /// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public abstract class Catalog : AjaxBase
    {
        private string paramName;
        private Type type;
        private ICatalogDAO catalogDao;
        
        /// <summary>
        /// Base contructor for all Catalog ajax type classes.
        /// </summary>
        /// <param name="type">The type of the entity</param>
        /// <param name="paramName">The request param name that will contain the entity info</param>
        /// <param name="catalogDao">The Catalog dao object</param>
        public Catalog(Type type, string paramName, ICatalogDAO catalogDao)
        {
            this.type = type;
            this.paramName = paramName;
            this.catalogDao = catalogDao;
        }

        /// <summary>
        /// Base contructor for all catalog ajax type classes
        /// </summary>
        /// <param name="type">The type of the entity</param>
        /// <param name="catalogDao">The Catalog dao object</param>
        public Catalog(Type type, ICatalogDAO catalogDao) : this(type, "entity", catalogDao)
        {
        }

        /// <summary>
        /// Generic method that returns a string json array of entities. 
        /// </summary>
        /// <param name="request">a <see cref="T:System.Web.HttpRequest"/></param>
        /// <returns>a JSON response</returns>
        public virtual string GetEntityList(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();
            try
            {
                Entity entity = (Entity)Activator.CreateInstance(type);
                list = catalogDao.GetEntities(entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return CreateEntityListResponse(list);
        }

        /// <summary>
        /// Generic method that returns a filtered string json array of entities.
        /// 
        /// Uses the filter information that comes in the request.
        /// </summary>
        /// <param name="request">a <see cref="T:System.Web.HttpRequest"/></param>
        /// <returns>a JSON response</returns>
        public virtual string GetFilteredEntities(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();
            FilterInfo filterInfo = null;
            try
            {                
                Entity entity = (Entity)Activator.CreateInstance(type);
                filterInfo = new FilterInfo(request);
                list = catalogDao.GetEntities(entity, filterInfo);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return CreateEntityListResponse(list, filterInfo);
        }

        /// <summary>
        /// Generic method that returns a filtered string json array of entities.
        /// 
        /// Uses the entity information that comes in the request.
        /// </summary>
        /// <param name="request">a <see cref="T:System.Web.HttpRequest"/></param>
        /// <returns>a JSON response</returns>
        public virtual string FindEntities(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();
            try
            {
                Entity entity = EntityFromJson(request);                
                list = catalogDao.FindEntities(entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return CreateEntityListResponse(list);
        }

        /// <summary>
        /// Generic method that returns a csv string list of entities.
        /// 
        /// Uses the filter information that comes in the request.
        /// </summary>
        /// <param name="request">a <see cref="T:System.Web.HttpRequest"/></param>
        /// <returns>a JSON response</returns>
        public virtual string ExportEntities(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = new List<Entity>();

            string response = "";
            try
            {
                bool allColumns = !string.IsNullOrEmpty(request.Params["allColumns"]) && bool.Parse(request.Params["allColumns"]) ? true : false;

                FilterInfo filterInfo = new FilterInfo(request);

                string filter = request.Params["filter"];
                FilterInfo.SearchType searchType = FilterInfo.SearchType.AND;
                if (!string.IsNullOrEmpty(filter) && filter == "OR")
                {
                    searchType = FilterInfo.SearchType.OR;
                }

                Entity entity = (Entity)Activator.CreateInstance(type);
                string json = request.Params[GetParamName()];
                LoggerHelper.Debug(GetParamName() + " = " + json);
                if (!string.IsNullOrEmpty(json))
                {
                    Dictionary<string, string> props = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(json);
                    entity.SetProperties(props);

                    list = GetCatalogDao().FindEntities(entity, filterInfo, searchType);
                }
                else
                {
                    int records = catalogDao.GetFilteredTotalRecords(entity, filterInfo);
                    filterInfo.Lenght = records;
                    list = catalogDao.GetEntities(entity, filterInfo);                    
                }
                
                response = EntityListToCSV(list, allColumns, entity, filterInfo);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return response.ToString();
        }

        /// <summary>
        /// Converts a list of entities to csv
        /// </summary>
        /// <param name="list">the entity list</param>
        /// <param name="allColumns">signal if all exportable columns will be included in the csv</param>
        /// <param name="entity">The entity type</param>
        /// <param name="filterInfo">the filter info</param>
        /// <returns>a csv response</returns>
        public virtual string EntityListToCSV(IList<Entity> list, bool allColumns, Entity entity, FilterInfo filterInfo)
        {
            string separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator; //Default
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ListSeparator"]))
            {
                separator = ConfigurationManager.AppSettings["ListSeparator"];
            }

            StringBuilder response = new StringBuilder();

            if (allColumns)
            {
                foreach (Field field in entity.GetFields())
                {
                    response.Append("\"").Append(field.Name).Append("\"").Append(separator);
                }
            }
            else
            {
                foreach (string col in filterInfo.ColumnsName)
                {
                    response.Append("\"").Append(col).Append("\"").Append(separator);
                }
            }

            response.Remove(response.Length - 1, 1);
            response.Append(System.Environment.NewLine);

            foreach (Entity ent in list)
            {
                if (allColumns)
                {
                    foreach (Field field in entity.GetFields())
                    {
                        response.Append("\"").Append(ent.GetProperty(field.Name).Replace("\"", "\"\"")).Append("\"").Append(separator);
                    }
                }
                else
                {
                    foreach (ColumnInfo col in filterInfo.Columns)
                    {
                        response.Append("\"").Append(ent.GetProperty(col.Name).Replace("\"", "\"\"")).Append("\"").Append(separator);
                    }
                }
                response.Remove(response.Length - separator.Length, separator.Length);
                response.Append(System.Environment.NewLine);
            }

            return response.ToString();
        }

        /// <summary>
        /// Generic method that saves an entity using the ICatalogDAO
        /// </summary>
        /// <param name="request">a <see cref="T:System.Web.HttpRequest"/></param>
        /// <returns>a JSON response</returns>
        public virtual string SaveEntity(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                Entity entity = EntityFromJson(request);

                BeforeSaveEntity(request, entity);
                catalogDao.SaveEntity(entity);
                AfterSaveEntity(request, entity);
            }
            catch(Exception e){
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse();
        }

        /// <summary>
        /// Executed before calling the dao save entity method
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="entity">The entity to be saved</param>
        public virtual void BeforeSaveEntity(HttpRequest request, Entity entity)
        {
        }

        /// <summary>
        /// Executed after calling the dao save entity method
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="entity">The entity to be saved</param>
        public virtual void AfterSaveEntity(HttpRequest request, Entity entity)
        {
        }

        /// <summary>
        /// Updates the specified entity.
        /// Obtaines the entity info from the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>a JSON response</returns>
        public virtual string UpdateEntity(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                Entity entity = EntityFromJson(request);
                catalogDao.UpdateEntity(entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }

            return SuccessResponse();
        }

        /// <summary>
        /// Generic method that deletes an entity using the ICatalogDAO
        /// </summary>
        /// <param name="request">The <see cref="T:System.Web.HttpRequest"/> that contains the entity that will be deleted.</param>
        /// <returns>a JSON response</returns>
        public virtual string DeleteEntity(HttpRequest request)
        {
            LoggerHelper.Info("Start");
            try
            {
                Entity entity = EntityFromJson(request);

                BeforeDeleteEntity(request, entity);
                catalogDao.DeleteEntity(entity);
                AfterDeleteEntity(request, entity);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                return ErrorResponse(e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }
            
            return SuccessResponse();
        }

        /// <summary>
        /// Executed before calling the dao delete entity
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="entity">The entity to be deleted</param>
        public virtual void BeforeDeleteEntity(HttpRequest request, Entity entity)
        {
        }

        /// <summary>
        /// Executed after calling the dao delete entity
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="entity">The entity to be deleted</param>
        public virtual void AfterDeleteEntity(HttpRequest request, Entity entity)
        {
        }

        /// <summary>
        /// Creates an entity from a JSON string obtaing from the specified request
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <returns>The created entity</returns>
        protected virtual Entity EntityFromJson(HttpRequest request) 
        {
            return EntityFromJson(request, null, null);
        }

        /// <summary>
        /// Creates an entity from a JSON string obtaing from the specified request using the specified
        /// enity type and the specified request param
        /// </summary>
        /// <param name="request">The HttpRequest</param>
        /// <param name="entType">The Entity type</param>
        /// <param name="param">The request parameter name</param>
        /// <returns>The created entity</returns>
        protected virtual Entity EntityFromJson(HttpRequest request, Type entType, string param)
        {
            string paramName = string.IsNullOrEmpty(param) ? this.paramName : param;
            Type t = entType == null ? this.type : entType;
            
            string json = request.Params[paramName];
            LoggerHelper.Debug(this.paramName + " = " + json);

            Entity entity = (Entity)Activator.CreateInstance(t);
            Dictionary<string, string> props = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(json);
            entity.SetProperties(props);
            return entity;
        } 

        /// <summary>
        /// ParamName accessor
        /// </summary>
        /// <returns>the reuest param name</returns>
        public virtual string GetParamName()
        {
            return this.paramName;
        }

        /// <summary>
        /// Type accesor
        /// </summary>
        /// <returns>The catalog entity type</returns>
        public virtual Type GetEntityType()
        {
            return this.type;
        }

        /// <summary>
        /// CatalogDAO accessor
        /// </summary>
        /// <returns>The catalog dao</returns>
        public virtual ICatalogDAO GetCatalogDao()
        {
            return this.catalogDao;
        }
    }
}