using System;
using System.IO;

namespace LTHDT2.Utils
{
    /// <summary>
    /// Simple Logger utility
    /// </summary>
    public static class Log
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LTHDT2",
            "app.log"
        );

        static Log()
        {
            try
            {
                var logDir = Path.GetDirectoryName(LogFilePath);
                if (logDir != null && !Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
            }
            catch
            {
                // Ignore errors during log initialization
            }
        }

        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public static void Error(string message)
        {
            WriteLog("ERROR", message);
        }

        public static void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        public static void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                
                // Also write to console in debug mode
                #if DEBUG
                Console.WriteLine(logMessage);
                #endif
            }
            catch
            {
                // Silently ignore logging errors
            }
        }
    }
}


