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

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace BinanceAPI
{
    internal class Queue
    {
        private SemaphoreSlim slim = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Processes the current Queue of messages
        /// </summary>
        /// <param name="loggedMessages">The Concurrent Queue of logged messages for this logger</param>
        /// <param name="path">The path to this loggers log file</param>
        /// <param name="logToConsole">True if this loggers messages should also be logged to the console</param>
        /// <param name="loglevel">The Log Level for the logger being processed</param>
        public void ProcessQueue(ConcurrentQueue<Message> loggedMessages, string path, bool logToConsole, LogLevel loglevel)
        {
            if (slim.Wait(0))
            {
                try
                {
                    if (loggedMessages.Count > 0)
                    {
                        bool message = loggedMessages.TryDequeue(out Message result);

                        if (result == null || message == false)
                        {
                            return;
                        }

                        if ((int)result.Level >= (int)loglevel)
                        {
                            File.AppendAllText(path, result.Content + "\n");

                            if (logToConsole) { Console.WriteLine(result.Content); }
                        }
                    }
                }
                catch { }
                finally
                {
                    slim.Release();
                }
            }
        }
    }
}
