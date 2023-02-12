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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceAPI
{
    /// <summary>
    /// The Logger
    /// <para>Can be prepared ahead of time if required</para>
    /// </summary>
    public class Logger
    {
        internal Timer? LogTimer;
        internal Queue MessageQueue = new Queue();
        internal ConcurrentQueue<Message> LoggedMessages = new ConcurrentQueue<Message>();
        internal bool TimerRunning = false;

        /// <summary>
        /// A Simple Logger
        /// </summary>
        /// <param name="path">Where to save the Log file</param>
        /// <param name="logToConsole">True if the message should also be logged to the console</param>
        /// <param name="dateFormat">The Date Format for messages used by this Logger</param>
        /// <param name="logLevel">The Log Level for this Logger</param>
        /// <param name="loggingInterval">
        /// How often the Logger should check for messages to write to the log
        /// </param>
        public Logger(string path, bool logToConsole = false, string dateFormat = "MM/dd/yy HH:mm:ss:fff", LogLevel logLevel = LogLevel.Error, int loggingInterval = 100)
        {
            LoggingInterval = loggingInterval;
            LogLevel = logLevel;
            DateFormat = dateFormat;
            LogPath = path;
            LogToConsole = logToConsole;
            Start();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Logger()
        {
            Dispose();
        }

        /// <summary>
        /// True if the Timer for this Logger should be Running
        /// <para>The LogTimer starts after the first message is logged</para>
        /// <para>You can stop the LogTimer by calling Reset() or Dispose()</para>
        /// </summary>
        public bool IsTimerRunning => TimerRunning;

        /// <summary>
        /// The LogPath for this Logger
        /// <para>Can only be set during Logger creation</para>
        /// </summary>
        public string LogPath { get; } = string.Empty;

        /// <summary>
        /// The current LoggingInterval for the Logger Timer
        /// <para>Can only be set during Logger creation</para>
        /// </summary>
        public int LoggingInterval { get; } = 100;

        /// <summary>
        /// The Log Level for this Logger
        /// <para>Default: LogLevel.Error</para>
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// True if this Logger should also Log messages to the Console
        /// <para>Optional</para>
        /// <para>Default: false</para>
        /// </summary>
        public bool LogToConsole { get; set; }

        /// <summary>
        /// The Date Format for messages used by this Logger
        /// <para>Optional</para>
        /// <para>Default: "MM/dd/yy HH:mm:ss:fff"</para>
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Write a Message to the Log
        /// </summary>
        /// <param name="message"> The Message</param>
        /// <param name="logLevel">The Log Level of the Message</param>
        internal void Write(string message, LogLevel logLevel = LogLevel.Error)
        {
            try
            {
                LoggedMessages.Enqueue(new Message(message, DateFormat, logLevel));

                if (!TimerRunning)
                {
                    Start();
                }
            }
            catch { }
        }

        internal void Start()
        {
            Task.Run(() =>
            {
                TimerRunning = true;
                LogTimer = new Timer(new TimerCallback((o) =>
                {
                    MessageQueue.ProcessQueue(LoggedMessages, LogPath, LogToConsole, LogLevel);
                }), null, 1, LoggingInterval);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Stop the Logger and Reset the Logger back to the Default State
        /// </summary>
        public void Reset()
        {
            Dispose();
            MessageQueue = new Queue();
            LoggedMessages = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// Stop the Logger by Disposing the LogTimer but don't reset the Logger back to the Default State
        /// </summary>
        public void Dispose()
        {
            try
            {
                TimerRunning = false;
                LogTimer?.Dispose();
            }
            catch { }
        }
    }
}
