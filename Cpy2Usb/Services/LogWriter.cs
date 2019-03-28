using System;
using System.Diagnostics;
using System.Reflection;
using Cpy2Usb.Properties;
using log4net;

namespace Cpy2Usb.Services
{
    public class LogWriter
    {
        public enum MsgType
        {
            Debug,

            Info,

            Warning,

            Error,

            Fatal
        }

        private static LogWriter _instance;

        private static readonly ILog Logger = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        public static LogWriter Instance => _instance ?? (_instance = new LogWriter());

        public void WriteMsg(string msg, MsgType type, int managedThreadId)
        {
            if (Settings.Default.VerboseLogs)
            {
                Debug.WriteLine(
                    "[DEBUG] {0:HH:mm:sst} |Thread ID# {3}|: [{1}] {2}",
                    DateTime.Now,
                    type,
                    msg,
                    managedThreadId);

                if (type == MsgType.Debug) Logger.Debug("[" + type + "] " + msg);
            }

            switch (type)
            {
                case MsgType.Info:
                    Logger.Info("[Thread #" + managedThreadId + "]  [" + type + "] " + msg);
                    break;
                case MsgType.Warning:
                    Logger.Warn("[Thread #" + managedThreadId + "]  [" + type + "] " + msg);
                    break;
                case MsgType.Error:
                    Logger.Error("[Thread #" + managedThreadId + "]  [" + type + "] " + msg);
                    break;
                case MsgType.Fatal:
                    Logger.Error("[Thread #" + managedThreadId + "]  [" + type + "] " + msg);
                    break;
            }
        }
    }
}