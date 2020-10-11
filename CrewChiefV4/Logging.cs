using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4
{
    static class Log
    {
        /// <summary>
        /// Class to encapsulate Console.Write, with LogType added to each call
        /// Each type can be turned on or off
        /// Each log has its type prepended before it is written to the console.
        /// </summary>
        [Flags]
        public enum LogType
        {
            FatalError = 1 << 0,
            Error = 1 << 1,
            Warning = 1 << 2,
            Commentary = 1 << 3,
            Subtitle = 1 << 4,
            Info = 1 << 5,
            Debug = 1 << 6,
            Verbose = 1 << 7,
        };
        private static LogType _logMask = setLogLevel(LogType.Verbose);
        private static readonly Dictionary<LogType, string> logPrefixes = new
            Dictionary<LogType, string>
        {
            { LogType.FatalError, "FATAL ERROR: " },
            { LogType.Error     , "ERROR:   " },
            { LogType.Warning   , "Warning: " },
            { LogType.Commentary, "Comment: " },
            { LogType.Subtitle,   "Subtitle:" },
            { LogType.Info      , "Info:    " },
            { LogType.Verbose   , "Verbose: " },
            { LogType.Debug     , "Debug:   " }
        };
        /// <summary>
        /// Set the log mask so all logs up to "logType" are shown
        /// </summary>
        /// <param name="logType"> log level</param>
        /// <returns></returns>
        public static LogType setLogLevel(LogType logType)
        {
            _logMask = 0;
            while (logType != 0)
            {
                _logMask |= logType;
                logType = (LogType)((int)logType / 2);
            }
            return _logMask;
        }
        /// <summary>
        /// Set the log mask so all logs matching "logMask" are shown
        /// Can set individual types of log, for example
        ///   setLogMask(LogType.Error | LogType.Subtitle);
        /// </summary>
        /// <param name="logMask"> log mask</param>
        /// <returns></returns>
        public static void setLogMask(LogType logMask)
        {
            _logMask = logMask;
        }

        /// <summary>
        /// Write "log" to Console if logType is enabled
        /// "log" has its type prepended before it is written.
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="log"></param>
        public static void _Log(LogType logType, string log)
        {
            if ((logType & _logMask) != 0 || CrewChief.Debugging)
            {
                Console.WriteLine(logPrefixes[logType] + log);
            }
            // tbd: Also write to log whatever the logLogMask
        }
        /// <summary>
        /// Write "log" with object to Console if LogType is enabled
        /// Example:
        ///   logLog(LogType.Warning, "Warning log {0}", 1.5f);
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="log">format string containing {0}</param>
        /// <param name="arg0">the object to replace {0}</param>
        public static void _Log(LogType logType, string log, object arg0)
        {
            _Log(logType, String.Format(log, arg0));
        }
        public static void _Log(LogType logType, string log, object arg0, object arg1)
        {
            _Log(logType, String.Format(log, arg0, arg1));
        }

        #region Shorthand calls
        /// <summary>
        /// Write "log" to Console if logType.FatalError is enabled
        /// </summary>
        /// <param name="log"></param>
        public static void Fatal(string log)
        {
            _Log(LogType.FatalError, log);
        }
        /// <summary>
        /// Write "log" to Console if logType.Error is enabled
        /// </summary>
        /// <param name="log"></param>
        public static void Error(string log)
        {
            _Log(LogType.Error, log);
        }
        /// <summary>
        /// Write "log" to Console if logType.Warning is enabled
        /// </summary>
        /// <param name="log"></param>
        public static void Warning(string log)
        {
            _Log(LogType.Warning, log);
        }
        public static void Warning(string log, object arg1, object arg2)
        {
            _Log(LogType.Warning, log, arg1, arg2);
        }
        /// <summary>
        /// Write "log" to Console if logType.Commentary is enabled
        /// </summary>
        /// <param name="log"></param>
        public static void Commentary(string log)
        {
            _Log(LogType.Commentary, log);
        }
        /// <summary>
        /// Write "log" to Console if logType.Subtitle is enabled
        /// </summary>
        /// <param name="log"></param>
        public static void Subtitle(string log)
        {
            _Log(LogType.Subtitle, log);
        }
        /// <summary>
        /// Write "log" to Console if logType.Info is enabled
        /// </summary>
        /// <param name="log"></param>
        public static void Info(string log)
        {
            _Log(LogType.Info, log);
        }
        /// <summary>
        /// Write "log" to Console if logType.Debug is enabled
        /// </summary>
        /// <param name="log"></param>
        public static void Debug(string log)
        {
            _Log(LogType.Debug, log);
        }
        /// <summary>
        /// Write "log" to Console if logType.Verbose is enabled
        /// </summary>
        /// <param name="log"></param>
        public static void Verbose(string log)
        {
            _Log(LogType.Verbose, log);
        }
        #endregion Shorthand calls
    }
}
