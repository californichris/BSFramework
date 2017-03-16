using BS.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Script.Serialization;
using BS.Common.Utils;


namespace BS.Common.Dao.Service
{
    public class ModulesServiceDAO : BaseDAO, IModulesDAO
    {
        //private FrameworkModulesService.ModulesServiceSoapClient client;

        public static readonly string AppNameKey = "EPEFrameworkAppName";

        private string AppName = ConfigurationManager.AppSettings[AppNameKey];
   
        private IPageInfoDAO sqlDAO;
       // private CacheItemPolicy policy = new CacheItemPolicy();
       
        public ModulesServiceDAO()
        {
            //client = GetServiceClient();
        }

        public IList<Entity> GetUserModules(string appName, string userLogin)
        {
            LoggerHelper.Info("Start");
            IList<Entity> list = null;
            try
            {
                //string json = client.GetUserModules(AppName, userLogin);
                //list = EntityUtils.DeserializeEntityList(json);
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
                throw new Exception("Unable to get modules list.", e);
            }
            finally
            {
                LoggerHelper.Info("End");
            }
            return list;
        }

        //protected virtual FrameworkModulesService.ModulesServiceSoapClient GetServiceClient()
        //{
        //    client = new FrameworkModulesService.ModulesServiceSoapClient();
        //    return client;
        //}

    }
}
