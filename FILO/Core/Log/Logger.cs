/*
    This file is part of FILO.

    FILO is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FILO is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FILO.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;

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
