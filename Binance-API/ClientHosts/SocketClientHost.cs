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

            MaxReconnectTries = options.MaxReconnectTries;

            StartSocketLog(options.LogPath, options.LogLevel, options.LogToConsole);
        }

        /// <summary>
        /// The maximum number of times to try to reconnect
        /// </summary>
        public int? MaxReconnectTries { get; protected set; }

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

        /// <summary>
        /// Connect to an url and listen for data
        /// </summary>
        /// <typeparam name="T">The type of the expected data</typeparam>
        /// <param name="url">The URL to connect to</param>
        /// <param name="request">The optional request object to send, will be serialized to json</param>
        /// <param name="authenticated">If the subscription is to an authenticated endpoint</param>
        /// <param name="dataHandler">The handler of update data</param>
        /// <param name="host">Socket Host</param>
        /// <param name="userStream">True if this socket is a user stream</param>
        /// <returns></returns>
        protected Task<CallResult<BaseSocketClient>> SubscribeAsync<T>(string url, BinanceSocketRequest request, bool authenticated, Action<DataEvent<T>> dataHandler, SocketClientHost host, bool userStream)
        {
            // Create Socket
            BaseSocketClient socketConnection = new BaseSocketClient(url, host);

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
        /// Sends the subscribe request and waits for a response to that request
        /// </summary>
        /// <param name="baseSocketClient">The connection to send the request on</param>
        /// <param name="request">The request to send, will be serialized to json</param>
        /// <returns></returns>
        protected internal async Task<CallResult<bool>> SubscribeAndWaitAsync(BaseSocketClient baseSocketClient, BinanceSocketRequest request)
        {
            CallResult<object>? callResult = null;
            await baseSocketClient.SendAndWaitAsync(request, TimeSpan.FromSeconds(3), data => HandleSubscriptionResponse(request, data, out callResult)).ConfigureAwait(false);

            if (callResult?.Success == true)
            {
                baseSocketClient.IsConfirmed = true;
            }

            return new CallResult<bool>(callResult?.Success ?? false, callResult == null ? new ServerError("No response on subscription request received") : callResult.Error);
        }

        /// <summary>
        /// The socketConnection received data (the data JToken parameter). The implementation of this method should check if the received data is a response to the subscription request that was send (the request parameter).
        /// For example; A subscribe request message is send with an Id parameter with value 10. The socket receives data and calls this method to see if the data it received is an
        /// anwser to any subscription request that was done. The implementation of this method should check if the response.Id == request.Id to see if they match (assuming the api has some sort of Id tracking on messages,
        /// if not some other method has be implemented to match the messages).
        /// If the messages match, the callResult out parameter should be set with the deserialized data in the from of (T) and return true.
        /// </summary>
        /// <param name="request">The request that the subscription sent</param>
        /// <param name="message">JToken</param>
        /// <param name="callResult">The interpretation (null if message wasn't a response to the request)</param>
        /// <returns>True if the message was a response to the subscription request</returns>
        public bool HandleSubscriptionResponse(BinanceSocketRequest request, JToken message, out CallResult<object>? callResult)
        {
            callResult = null;
            if (message.Type != JTokenType.Object)
                return false;

            var id = message["id"];
            if (id == null)
                return false;

            if ((int)id != request.Id)
                return false;

            var result = message["result"];
            if (result != null && result.Type == JTokenType.Null)
            {
                callResult = new CallResult<object>(null, null);
                return true;
            }

            var error = message["error"];
            if (error == null)
            {
                callResult = new CallResult<object>(null, new ServerError("Unknown error: " + message.ToString()));
                return true;
            }

            callResult = new CallResult<object>(null, new ServerError(error["code"]!.Value<int>(), error["msg"]!.ToString()));
            return true;
        }

        /// <summary>
        /// Needs to check if a received message matches a handler by request. After subscribing data message will come in. These data messages need to be matched to a specific connection
        /// to pass the correct data to the correct handler. The implementation of this method should check if the message received matches the subscribe request that was sent.
        /// </summary>
        /// <param name="message">The received data</param>
        /// <param name="request">The subscription request</param>
        /// <returns>True if the message is for the subscription which sent the request</returns>
        public bool MessageMatchesHandler(JToken message, BinanceSocketRequest request)
        {
            if (message.Type != JTokenType.Object)
                return false;

            var stream = message["stream"];
            if (stream == null)
                return false;

            return request.Params.Contains(stream.ToString());
        }

        /// <summary>
        /// Needs to unsubscribe a subscription, typically by sending an unsubscribe request.
        /// </summary>
        /// <param name="socketClient">The connection on which to unsubscribe</param>
        /// <param name="Request">The Socket Request</param>
        /// <returns></returns>
        public async Task<bool> UnsubscribeAsync(BaseSocketClient socketClient, BinanceSocketRequest Request)
        {
            var returnresult = false;
            var unsub = new BinanceSocketRequest
            {
                Method = "UNSUBSCRIBE",
                Params = Request.Params,
                Id = Request.Id,
            };

            if (!socketClient.IsOpen)
            {
                return false;
            }

            await socketClient.SendAndWaitAsync(unsub, TimeSpan.FromSeconds(3),
                data =>
                {
                    if (data.Type != JTokenType.Object)
                    {
                        return false;
                    }

                    var id = data["id"];
                    if (id == null)
                    {
                        return false;
                    }

                    if ((int)id != unsub.Id)
                    {
                        return false;
                    }

                    var result = data["result"];
                    if (result?.Type == JTokenType.Null)
                    {
                        returnresult = true;
                        return true;
                    }

                    return false;
                }).ConfigureAwait(false);
            return returnresult;
        }

        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="client">The Base Socket Client to Ubsubscribe</param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(BaseSocketClient client)
        {
#if DEBUG
            SocketLog?.Info("Closing subscription " + client.Request.Id);
#endif
            await client.CloseAndDisposeSubscriptionAsync().ConfigureAwait(false);
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
