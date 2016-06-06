using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace BS.Common.Utils
{
    /// <summary>
    /// This class consists of static utility methods to help with file operations.
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Saves the specified data into the specified file path. 
        /// This method is thread save
        /// </summary>
        /// <param name="data">The data to be saved</param>
        /// <param name="filePath">The path to the file</param>        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SaveToFile(string data, string filePath)
        {
            StreamWriter stream = null;
            try
            {
                LoggerHelper.Debug("Path: " + filePath);
                stream = new StreamWriter(filePath, false);
                stream.Write(data);
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Not able to save data to file", e);
            }
            finally
            {
                stream.Dispose();
            }
        }

        /// <summary>
        /// Loads the data from the specified file path.
        /// The file is loaded in readonly mode.
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>The loaded data</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file is not found</exception>
        public static string LoadFile(string path)
        {
            string json = "[]";
            try
            {
                json = System.IO.File.ReadAllText(@path);
                LoggerHelper.Debug("Done reading file.");
            }
            catch (System.IO.FileNotFoundException e)
            {
                LoggerHelper.Error(e);
                throw;
            }

            return json;
        }
    }
}
