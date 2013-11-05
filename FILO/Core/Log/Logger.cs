using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FILO.Core.Log
{
    public static class Logger
    {
        public struct MessageData
        {
            public string Message;
            public string FormatedMessage;
            public DateTime Time;
            public MessageType Type;
        }

        public static string LastMessage { get; private set; }

        private static readonly List<ILogger> Loggers = new List<ILogger>();
        private const string DateTimeFormat = "T";

        public static void Info(string message)
        {
            string formatedMessage;
            DateTime time;
            formatMessage(message, out formatedMessage, out time);
            _log(message, formatedMessage, time, MessageType.Info);
        }

        public static void Warning(string message)
        {
            string formatedMessage;
            DateTime time;
            formatMessage(message, out formatedMessage, out time);
            _log(message, formatedMessage, time, MessageType.Warning);   
        }

        public static void Error(string message)
        {
            string formatedMessage;
            DateTime time;
            formatMessage(message, out formatedMessage, out time);
            _log(message, formatedMessage, time, MessageType.Error);
        }

        public static void Error(Exception e)
        {
            Error("Caught Exception: " + e.Message + "\n" + e.ToString());
        }

        public static void Debug(string message)
        {
            string formatedMessage;
            DateTime time;
            formatMessage(message, out formatedMessage, out time);
            _log(message, formatedMessage, time, MessageType.Debug);
        }

        private static void formatMessage(string message, out string formatedMessage, out DateTime time)
        {
            if (message == null)
                throw new ArgumentException("message is null!");
            time = DateTime.Now;
            formatedMessage = "[" + time.ToString(DateTimeFormat) + "] " + message;
        }

        private static void _log(string message, string formatedMessage, DateTime time, MessageType type)
        {
            var md = new MessageData();
            md.Message = message;
            md.FormatedMessage = formatedMessage;
            md.Time = time;
            md.Type = type;

            Loggers.ForEach((l) => l.OnLog(md));

            LastMessage = message;
        }
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Debug
    }
}
