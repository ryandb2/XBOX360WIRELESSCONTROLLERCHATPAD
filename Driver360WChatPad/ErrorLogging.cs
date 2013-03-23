using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Driver360WChatPad
{
    public static class ErrorLogging
    {
        public static StreamWriter logFile = new System.IO.StreamWriter(String.Format("errorLog{0}{1}{2}.log", System.DateTime.UtcNow.Year, System.DateTime.UtcNow.Month, System.DateTime.UtcNow.Day), true);
        public enum LogLevel
        {
            Information,
            Debug,
            Warning,
            Error,
            Fatal
        }
        public static void WriteLogEntry(string message, LogLevel logLevel)
        {
            logFile.WriteLine(String.Format("{0} - {1}  {2}", System.DateTime.UtcNow.ToString(),logLevel.ToString(),message));
            logFile.Flush();
        }
    }
}
