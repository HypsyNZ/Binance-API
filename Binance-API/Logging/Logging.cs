//******************************************************************************************************
//  Copyright © 2022, S. Christison. No Rights Reserved.
//
//  Licensed to [You] under one or more License Agreements.
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//
//******************************************************************************************************

using BinanceAPI.Enums;
using System;

namespace BinanceAPI
{
    /// <summary>
    /// The Log
    /// </summary>
    public partial class Logging
    {
        private Logger Logger;

        /// <summary>
        /// A Simple Log using the Default Logger
        /// </summary>
        /// <param name="path">Where to save the Log file</param>
        /// <param name="logToConsole">True if the message should also be logged to the console</param>
        /// <param name="dateFormat">The Date Format for messages used by this Logger</param>
        /// <param name="logLevel">The Log Level for this Logger</param>
        /// <param name="loggingInterval">How often the Logger should check for messages to write to the log</param>
        public Logging(string path, bool logToConsole = false, string dateFormat = "MM/dd/yy HH:mm:ss:fff", LogLevel logLevel = LogLevel.Error, int loggingInterval = 100)
        {
            Logger = new Logger(path, logToConsole, dateFormat, logLevel, loggingInterval);
        }

        /// <summary>
        /// A Simple Log using a Logger you prepared earlier
        /// </summary>
        public Logging(Logger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Logging()
        {
            Dispose();
        }

        /// <summary>
        /// Write a message to the Log with LogLevel Trace
        /// </summary>
        /// <param name="message"></param>
        public void Trace(string message)
        {
            Logger.Write("[Trace] " + message, LogLevel.Trace);
        }

        /// <summary>
        /// Write a message to the Log with LogLevel Debug
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message)
        {
            Logger.Write("[Debug] " + message, LogLevel.Debug);
        }

        /// <summary>
        /// Write a message to the Log with LogLevel Information
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            Logger.Write("[Info] " + message, LogLevel.Information);
        }

        /// <summary>
        /// Write a message to the Log with LogLevel Warning
        /// </summary>
        /// <param name="message"></param>
        public void Warning(string message)
        {
            Logger.Write("[Warn] " + message, LogLevel.Warning);
        }

        /// <summary>
        /// Write a message to the Log with LogLevel Error
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
        {
            Logger.Write("[Error] " + message, LogLevel.Error);
        }

        /// <summary>
        /// Write a message to the Log with LogLevel Critical
        /// </summary>
        /// <param name="message"></param>
        public void Critical(string message)
        {
            Logger.Write("[Critical] " + message, LogLevel.Critical);
        }

        /// <summary>
        /// Write an Exception to the Log with LogLevel Trace
        /// </summary>
        /// <param name="ex"></param>
        public void Trace(Exception ex)
        {
            Logger.Write("[Trace] Exception: " + ex.Message + "" +
                "| Trace: " + ex.StackTrace +
                "| Inner: " + ex.InnerException, LogLevel.Trace);
        }

        /// <summary>
        /// Write a Message and Exception to the Log with LogLevel Trace
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Trace(string message, Exception ex)
        {
            Logger.Write("[Trace] " + message + " " +
                "| Exception: " + ex.Message + "" +
                "| Trace: " + ex.StackTrace +
                "| Inner: " + ex.InnerException, LogLevel.Trace);
        }

        /// <summary>
        /// Write an Exception to the Log with LogLevel Debug
        /// </summary>
        /// <param name="ex"></param>
        public void Debug(Exception ex)
        {
            Logger.Write("[Debug] Exception: " + ex.Message + "" +
                "| Trace: " + ex.StackTrace +
                "| Inner: " + ex.InnerException, LogLevel.Debug);
        }

        /// <summary>
        /// Write a Message and Exception to the Log with LogLevel Debug
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Debug(string message, Exception ex)
        {
            Logger.Write("[Debug] " + message + " " +
                "| Exception: " + ex.Message + "" +
                "| Trace: " + ex.StackTrace +
                "| Inner: " + ex.InnerException, LogLevel.Debug);
        }

        /// <summary>
        /// Write an Exception to the Log with LogLevel Error
        /// </summary>
        /// <param name="ex"></param>
        public void Error(Exception ex)
        {
            Logger.Write("[Error] Exception: " + ex.Message + "" +
                "| Trace: " + ex.StackTrace +
                "| Inner: " + ex.InnerException, LogLevel.Error);
        }

        /// <summary>
        /// Write a Message and Exception to the Log with LogLevel Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex)
        {
            Logger.Write("[Error] " + message + " " +
                "| Exception: " + ex.Message + "" +
                "| Trace: " + ex.StackTrace +
                "| Inner: " + ex.InnerException, LogLevel.Error);
        }

        /// <summary>
        /// Write an Exception to the Log with LogLevel Critical
        /// </summary>
        /// <param name="ex"></param>
        public void Critical(Exception ex)
        {
            Logger.Write("[Critical] Exception: " + ex.Message + "" +
                "| Trace: " + ex.StackTrace +
                "| Inner: " + ex.InnerException, LogLevel.Critical);
        }

        /// <summary>
        /// Write a Message and Exception to the Log with LogLevel Critical
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Critical(string message, Exception ex)
        {
            Logger.Write("[Critical] " + message + " " +
                "| Exception: " + ex.Message + "" +
                "| Trace: " + ex.StackTrace +
                "| Inner: " + ex.InnerException, LogLevel.Critical);
        }

        /// <summary>
        /// Get/Set the LogLevel for the current Logger
        /// </summary>
        public LogLevel LogLevel { get => Logger.LogLevel; set => Logger.LogLevel = value; }

        /// <summary>
        /// Get/Set whether the current Logger should Log messages to Console
        /// </summary>
        public bool LogToConsole { get => Logger.LogToConsole; set => Logger.LogToConsole = value; }

        /// <summary>
        /// Get/Set the Date Format for the current Logger
        /// </summary>
        public string DateFormat { get => Logger.DateFormat; set => Logger.DateFormat = value; }

        /// <summary>
        /// Stop the current Logger during Dispose of the Log
        /// </summary>
        public void Dispose()
        {
            Logger?.Dispose();
        }
    }
}
