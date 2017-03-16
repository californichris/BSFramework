using System;
using System.Configuration;
using System.Reflection;
using BS.Common.Dao;

namespace BS.Common.Utils
{
    /// <summary>
    /// This class consists of static utility methods to help with DAOFactory operations
    /// </summary>
    public static class FactoryUtils
    {
        /// <summary>
        /// The application setting key that contains the name of the connection string that will be used.
        /// </summary>
        public static readonly string DBConfigParamName = "DataBaseConnString";

        /// <summary>
        /// Returns an instance of the specified clazz. The class must implement the IBaseDAO interface.
        /// </summary>
        /// <param name="clazz">The fully qualified name of the class to be instantiated</param>
        /// <returns>an instance of the specified clazz </returns>
        public static IBaseDAO GetDAO(string clazz)
        {
            return GetDAO(clazz, null);
        }

        /// <summary>
        /// Returns an instance of the specified clazz with the specified dbConnString.
        /// The class must implement the IBaseDAO interface. If the dbConnString argument is null
        /// the default connection string will be used.
        /// </summary>
        /// <param name="clazz">The fully qualified name of the class to be instantiated</param>
        /// <param name="dbConnString">The Database connection string</param>
        /// <returns>an instance of the specified clazz </returns>
        public static IBaseDAO GetDAO(string clazz, string dbConnString)
        {
            if (string.IsNullOrEmpty(clazz))
            {
                LoggerHelper.Error("Class is not specified.");
                throw new ArgumentNullException("Class is not specified.");
            }

            if (string.IsNullOrEmpty(dbConnString))
            {
                LoggerHelper.Debug("DB ConnString empty using default.");
                dbConnString = ConfigurationManager.AppSettings[DBConfigParamName];
            }

            Type type = Type.GetType(clazz);
            ConstructorInfo constInfo = type.GetConstructor(new Type[] { typeof(string) });

            IBaseDAO instance = null;
            if (constInfo == null)
                instance = (IBaseDAO) Activator.CreateInstance(type);
            else
                instance = (IBaseDAO) constInfo.Invoke(new Object[] { dbConnString });           

            return instance;
        }        
    }
}
