/*
*MIT License
*
*Copyright (c) 2023 S Christison
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:
*
*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
*SOFTWARE.
*/

using BinanceAPI.ClientBase;
using BinanceAPI.Objects;
using BinanceAPI.Objects.Other;
using BinanceAPI.Sockets;
using BinanceAPI.SocketSubClients;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BinanceAPI.TheLog;

namespace BinanceAPI.ClientHosts
{
    /// <summary>
    /// Base for socket client implementations
    /// </summary>
    public class SocketClientHost : BaseClient
    {
        private int _maxReconnectTries { get; set; } = 50;

        /// <summary>
        /// Spot Stream Endpoints
        /// </summary>
        public BinanceSocketClientSpot Spot { get; set; }

        /// <summary>
        /// The Default Options or the Options that you Set
        /// <para>new BinanceSocketClientOptions() creates the standard defaults regardless of what you set this to</para>
        /// </summary>
        public static SocketClientHostOptions DefaultOptions = new();

        /// <summary>
        /// Create a new instance of BinanceSocketClient with default options
        /// </summary>
        public SocketClientHost() : this(DefaultOptions)
        {
        }

        /// <summary>
        /// Create a new instance of BinanceSocketClient using provided options
        /// </summary>
        /// <param name="options">The options to use for this client</param>
        public SocketClientHost(SocketClientHostOptions options) : base(options)
        {
            Spot = new BinanceSocketClientSpot(this);

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _maxReconnectTries = options.MaxReconnectTries;
            StartSocketLog(options.LogPath, options.LogLevel, options.LogToConsole);
        }

        /// <summary>
        /// Set the default options to be used when creating new socket clients
        /// </summary>
        /// <param name="options"></param>
        public static void SetDefaultOptions(SocketClientHostOptions options)
        {
            DefaultOptions = options;
        }

        internal Task<CallResult<BaseSocketClient>> SubscribeInternal<T>(string url, IEnumerable<string> topics, Action<DataEvent<T>> onData, bool userStream)
        {
            BinanceSocketRequest request = new BinanceSocketRequest
            {
                Method = "SUBSCRIBE",
                Params = topics.ToArray(),
                Id = NextId()
            };

            return SubscribeAsync(url, request, false, onData, this, userStream);
        }

        protected private Task<CallResult<BaseSocketClient>> SubscribeAsync<T>(string url, BinanceSocketRequest request, bool authenticated, Action<DataEvent<T>> dataHandler, SocketClientHost host, bool userStream)
        {
            // Create Socket
            BaseSocketClient socketConnection = new BaseSocketClient(url, _maxReconnectTries);

            void InternalHandlerUserStream(JToken messageEvent)
            {
                if (typeof(T) == typeof(string))
                {
                    var stringData = (T)Convert.ChangeType(messageEvent.ToString(), typeof(T));

                    dataHandler(new DataEvent<T>(stringData, null));
                    return;
                }
            }


            void InternalHandler(JToken messageEvent)
            {
                var desResult = Json.Deserialize<T>(messageEvent);

                if (!desResult)
                {
#if DEBUG
                    SocketLog?.Warning($"Socket {request.Id} Failed to deserialize data into type {typeof(T)}: {desResult.Error}");
#endif
                    return;
                }

                dataHandler(new DataEvent<T>(desResult.Data, null));
            }

            socketConnection.Request = request;
            socketConnection.IsConfirmed = true;

            if (userStream)
            {
                socketConnection.MessageHandler = InternalHandlerUserStream;
            }
            else
            {
                socketConnection.MessageHandler = InternalHandler;
            }

            return Task.FromResult(new CallResult<BaseSocketClient>(socketConnection, null));
        }

        /// <summary>
        /// Dispose the client
        /// </summary>
        public override void Dispose()
        {
#if DEBUG
            SocketLog?.Debug("Disposing socket client, closing all subscriptions");
#endif
            base.Dispose();
        }
    }
}
