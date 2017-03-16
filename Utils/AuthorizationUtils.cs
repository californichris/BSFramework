using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using BS.Common.Dao;
using BS.Common.Entities;
using BS.Common.Entities.Page;
using BS.Common.Dao.Service;

namespace BS.Common.Utils
{
    public class AuthorizationUtils
    {
        public static readonly string CurrentUserLogin = "CurrentUserLogin";
        public static readonly string CurrentUserModules = "CurrentUserModules";
        public static readonly string AppNameKey = "EPEFrameworkAppName";
        private static string AppName = ConfigurationManager.AppSettings[AppNameKey];

        public static string GetUserLogin()
        {
            return GetUserLogin(System.Web.HttpContext.Current.Session);
        }

        public static string GetUserLogin(System.Web.SessionState.HttpSessionState session)
        {
            string loginName = "";
            if (session[CurrentUserLogin] == null)
            {
                loginName = GetCurrentUserLogin();
                if (!string.IsNullOrEmpty(loginName))
                {
                    session[CurrentUserLogin] = loginName;
                }
            }
            else
            {
                loginName = (string) session[CurrentUserLogin];
            }

            return loginName;
        }


        public static string GetCurrentUserLogin()
        {
            return GetCurrentUserLogin(System.Web.HttpContext.Current.User);
        }

        public static string GetCurrentUserLogin(System.Security.Principal.IPrincipal user)
        {
            string loginName = "";
            try
            {
                string[] name = { "" };
                if (!string.IsNullOrEmpty(user.Identity.Name))
                {
                    name = user.Identity.Name.Split('\\');
                    loginName = user.Identity.Name;
                }

                LoggerHelper.Debug("loginName before split = [" + loginName + "]");

                if (name.Length >= 1)
                {
                    loginName = name[1];
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Error(e);
            }

            if (!string.IsNullOrEmpty(loginName))
            {
                loginName = loginName.ToLower();
            }

            LoggerHelper.Debug("final loginName = [" + loginName + "]");
            return loginName;
        }

        private static IPageInfoDAO GetPageInfoDAO()
        {
            return (IPageInfoDAO) FactoryUtils.GetDAO(ConfigurationManager.AppSettings["IPageInfoDAO"], "");
        }

        private static ICatalogDAO GetCatalogDAO()
        {
            BaseSqlDAO dao = (BaseSqlDAO) FactoryUtils.GetDAO(ConfigurationManager.AppSettings["ICatalogDAO"], "");
            dao.SetQueryBuilder(DbUtils.GetQueryBuilder());

            return (ICatalogDAO) dao;
        }

        private static Page GetPage(string pageName)
        {
            return GetPage(GetPageInfoDAO(), pageName);
        }

        private static Page GetPage(IPageInfoDAO pageDAO, string pageName)
        {
            return pageDAO.GetPageConfig("", pageName);
        }
        
        public static IList<Entity> GetUserModules()
        {          
            LoggerHelper.Debug("Getting user roles.");
            if (System.Web.HttpContext.Current.Session[CurrentUserModules] == null)
            {
                IModulesDAO modulesDAO = new ModulesServiceDAO();
                IList<Entity> roleMods = modulesDAO.GetUserModules(AppName, GetUserLogin());

                System.Web.HttpContext.Current.Session[CurrentUserModules] = roleMods;
            }

            return (IList<Entity>)System.Web.HttpContext.Current.Session[CurrentUserModules];
        }

        public static bool UserHavePermissions()
        {
            return UserHavePermissions(GetRequestPage(), GetUserModules());
        }

        public static bool UserHavePermissions(string path, IList<Entity> userModules)
        {
            if (path.Equals("InvalidAccess.aspx", StringComparison.InvariantCultureIgnoreCase)) return true;

            foreach (Entity userMod in userModules)
            {
                if (path.Equals(userMod.GetProperty("URL"), StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static Entity GetModule(string path, IList<Entity> modules)
        {
            Entity module = ((List<Entity>)modules).Find(x => x.GetProperty("URL").Equals(path, StringComparison.InvariantCultureIgnoreCase));

            return module;
        }

        public static string SerializeModules(IList<Entity> roleMods)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            StringBuilder modules = new StringBuilder();

            foreach (Entity ent in roleMods)
            {
                modules.Append(ser.Serialize(ent.GetProperties())).Append(",");
            }

            if (modules.Length > 0)
            {
                modules.Remove(modules.Length - 1, 1);
            }

            return modules.ToString();
        }

        private static string GetRequestPage()
        {
            string reqPath = System.Web.HttpContext.Current.Request.Path;
            reqPath = reqPath.ToLower().Replace(System.Web.HttpContext.Current.Request.ApplicationPath.ToLower() + "/", "");

            return reqPath;
        }

        public static void AppendModulesInfo(StringBuilder menuGlobals)
        {
            AppendModulesInfo(menuGlobals, GetRequestPage(), GetUserModules());
        }

        public static void AppendModulesInfo(StringBuilder menuGlobals, string path, IList<Entity> modules)
        {
            if (modules.Count <= 0) return;

            menuGlobals.Append("const USER_MODULES = '[").Append(AuthorizationUtils.SerializeModules(modules)).Append("]';\n");
           
            Entity module = AuthorizationUtils.GetModule(path, modules);
            if (module != null)
            {
                menuGlobals.Append("const NEW_ACCESS = ").Append(module.GetProperty("NewAccess").ToLower()).Append(";\n");
                menuGlobals.Append("const EDIT_ACCESS = ").Append(module.GetProperty("EditAccess").ToLower()).Append(";\n");
                menuGlobals.Append("const DELETE_ACCESS = ").Append(module.GetProperty("DeleteAccess").ToLower()).Append(";\n");
                menuGlobals.Append("const ROLE_NAME = '").Append(module.GetProperty("RoleName")).Append("';\n");
            }

        }
    }
}
