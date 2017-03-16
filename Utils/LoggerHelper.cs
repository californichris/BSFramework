using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace BS.Common.Utils
{
	/// <summary>
	/// Helper class for messages and events that are logged to the
	/// application logging block
	/// </summary>
    /// <history>
    ///     <change date="07/31/2013" author="Christian Beltran">
    ///         Initial Version.
    ///     </change>
    /// </history>
    public static class LoggerHelper
	{
        /// <summary>
        /// Logs the specified message with the specified log level
        /// </summary>
        /// <param name="message">The Message to be log</param>
        /// <param name="severity">The log level</param>
        public static void Log(string message, TraceEventType severity)
        {
            IDictionary<string, object> extraInfo = new Dictionary<string, object>();
            StackFrame stackFrame = new StackFrame(2, true);
            string methodFullName = stackFrame.GetMethod().DeclaringType.FullName + "." + stackFrame.GetMethod().Name;
            extraInfo.Add("MethodFullName", methodFullName);

            Logger.Write(new LogEntry
            {
                Message = message,
                Severity = severity,
                ExtendedProperties = extraInfo
            });
        }

        /// <summary>
        /// Logs the specified message concatenating the exception message and the 
        /// exception stacktrace with a Error log level.
        /// </summary>
        /// <param name="message">The message to be log</param>
        /// <param name="exc">The exception to be log</param>
		public static void Error(string message, Exception exc)
		{
            string logMessage = message;

            if (exc != null)
            {
                logMessage = logMessage + exc.Message;
                logMessage += " [" + exc.StackTrace + "]";
            }

            Log(logMessage, TraceEventType.Error);
		}

        /// <summary>
        /// Logs the specified exception with a Error log level.
        /// </summary>
        /// <param name="exc">The exception to be log</param>
		public static void Error(Exception exc)
		{
            Error("", exc);
		}

        /// <summary>
        /// Logs the specified message with a Error log level.
        /// </summary>
        /// <param name="message">The message to be log</param>
        public static void Error(string message)
        {
            Error(message, null);
        }

        /// <summary>
        /// Logs the specified message with a Debug log level.
        /// </summary>
        /// <param name="message">The message to be log</param>
		public static void Debug(string message)
		{
            Log(message, TraceEventType.Verbose);
		}

        /// <summary>
        /// Logs the specified message with a Information log level.
        /// </summary>
        /// <param name="message">The message to be log</param>
        public static void Info(string message)
        {
            Log(message, TraceEventType.Information);
        }

        /// <summary>
        /// Logs the specified message with a Warning log level.
        /// </summary>
        /// <param name="message">The message to be log</param>
        public static void Warning(string message)
        {
            Log(message, TraceEventType.Warning);
        }

	}
}
