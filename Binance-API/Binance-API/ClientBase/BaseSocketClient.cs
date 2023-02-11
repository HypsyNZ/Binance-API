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

using BinanceAPI.ClientHosts;
using BinanceAPI.Enums;
using BinanceAPI.Objects;
using BinanceAPI.Objects.Other;
using BinanceAPI.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static BinanceAPI.TheLog;

namespace BinanceAPI.ClientBase
{
    /// <summary>
    /// The Base Socket Client
    /// <para>Created Automatically when you Subscribe to a SocketSubscription</para>
    /// <para>Wraps the Underlying ClientWebSocket</para>
    /// </summary>
    public class BaseSocketClient
    {
        private const string SOCKET_CLOSE_MESSAGE = "] received `Close` message..";
        private const string LEFT_BRACKET = "[";

        protected private bool sendLoopActive = false;
        protected private bool digestLoopActive = false;

        /// <summary>
        /// Event - Connectiion restored event
        /// </summary>
        public event Action<TimeSpan>? ConnectionRestored;

        /// <summary>
        /// Event - Occurs when the status of the socket changes
        /// </summary>
        public event Action<ConnectionStatus>? ConnectionStatusChanged;

        /// <summary>
        /// Action - Occurs when a message is processed
        /// </summary>
        public Action<JToken>? MessageHandler { get; set; } = delegate { };

        private readonly SocketClientHost socketClient;
        private readonly List<PendingRequest> pendingRequests;

        private Encoding _encoding = Encoding.UTF8;
        private AsyncResetEvent _sendEvent = new AsyncResetEvent();
        private ConcurrentQueue<byte[]> _sendBuffer = new ConcurrentQueue<byte[]>();

        private volatile bool _resetting;
        private volatile bool _multiPartMessage = false;
        private ArraySegment<byte> _buffer = new();

        [AllowNull]
        private volatile MemoryStream _memoryStream = null;

        [AllowNull]
        private volatile WebSocketReceiveResult _receiveResult = null;

        [AllowNull]
        internal BinanceSocketRequest Request { get; set; }

        /// <summary>
        /// The Real Underlying Socket
        /// </summary>
        public ClientWebSocket ClientSocket { get; protected set; } = null!;

        /// <summary>
        /// The Current Status of the Socket Connection
        /// </summary>
        public ConnectionStatus SocketConnectionStatus { get; set; }

        /// <summary>
        /// If the socket should be reconnected upon closing
        /// </summary>
        public bool ShouldAttemptConnection { get; set; } = true;

        /// <summary>
        /// Time of disconnecting
        /// </summary>
        public DateTime? DisconnectTime { get; set; }

        /// <summary>
        /// Url this socket connects to
        /// </summary>
        public string Url { get; internal set; }

        internal bool IsConfirmed { get; set; }

        /// <summary>
        /// If the connection is open
        /// </summary>
        public bool IsOpen => ClientSocket.State == WebSocketState.Open && !_resetting;

        /// <summary>
        /// Encoding used for decoding the received bytes into a string
        /// </summary>
        public Encoding Encoding
        {
            get => _encoding;
            set
            {
                _encoding = value;
            }
        }

        /// <summary>
        /// The Base Client for handling a SocketSubscription
        /// </summary>
        /// <param name="url">The url the socket should connect to</param>
        /// <param name="client"></param>
        internal BaseSocketClient(string url, SocketClientHost client)
        {
            Url = url;

            NewWebSocketInternal();

            socketClient = client;

            pendingRequests = new List<PendingRequest>();

            DisconnectTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Unsubscribe and Dispose the Socket
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UnsubscribeAsync()
        {
            bool b = await socketClient.UnsubscribeAsync(this, Request).ConfigureAwait(false);
            await DisposeAsync().ConfigureAwait(false);
            return b;
        }

        /// <summary>
        /// Closes the subscription on this connection
        /// <para>Closes the connection if the correct subscription is provided</para>
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CloseAndDisposeSubscriptionAsync()
        {
            if (IsConfirmed)
            {
                MessageHandler = null;
                var result = await socketClient.UnsubscribeAsync(this, Request).ConfigureAwait(false);
                if (result)
                {
                    SocketLog?.Debug($"Socket {Request.Id} subscription successfully unsubscribed, Closing connection..");
                }
                else
                {
                    SocketLog?.Debug($"Socket {Request.Id} subscription seems to already be unsubscribed, Closing connection..");
                }

                await DisposeAsync().ConfigureAwait(false);

                return result;
            }

            return false;
        }

        /// <summary>
        /// Close and Dispose the Socket and all Message Handlers
        /// </summary>
        public async Task DisposeAsync()
        {
            ShouldAttemptConnection = false;
            await ReconnectSocketAsync().ConfigureAwait(false);
            DisconnectTime = DateTime.UtcNow;
#if DEBUG
            SocketLog?.Trace($"Socket {Request.Id} disposed..");
#endif
        }

        /// <summary>
        /// Send data and wait for an answer
        /// </summary>
        /// <param name="obj">The object to send</param>
        /// <param name="timeout">The timeout for response</param>
        /// <param name="handler">The response handler</param>
        /// <returns></returns>
        public Task SendAndWaitAsync(BinanceSocketRequest obj, TimeSpan timeout, Func<JToken, bool> handler)
        {
            if (_resetting)
            {
                return Task.CompletedTask;
            }

            var pending = new PendingRequest(handler, timeout);
            lock (pendingRequests)
            {
                pendingRequests.Add(pending);
            }

#if DEBUG
            SocketLog?.Trace($"Socket {Request.Id} Adding to sent buffer..");
#endif
            _sendBuffer.Enqueue(Encoding.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })));
            _sendEvent.Set();

            return pending.Event.WaitAsync(timeout);
        }

        /// <summary>
        /// Loop for sending data
        /// </summary>
        /// <returns></returns>
        protected private async Task SendLoopAsync()
        {
            try
            {
                sendLoopActive = true;

                while (!_resetting)
                {
                    await _sendEvent.WaitAsync().ConfigureAwait(false);

                    while (!_resetting)
                    {
                        bool s = _sendBuffer.TryDequeue(out var data);
                        if (s)
                        {
                            await ClientSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, default).ConfigureAwait(false);
#if DEBUG
                            SocketLog?.Trace($"Socket {Request.Id} sent {data.Length} bytes..");
#endif
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                sendLoopActive = false;
            }
            catch (Exception ex)
            {
                TheLog.SocketLog?.Error(ex);
                sendLoopActive = false;
                await ReconnectSocketAsync().ConfigureAwait(false);
            }
        }

        protected private async Task DigestLoopAsync()
        {
            try
            {
                digestLoopActive = true;

                while (!_resetting)
                {
                    _buffer = new ArraySegment<byte>(new byte[8192]);
                    _multiPartMessage = false;
                    _memoryStream = null;
                    _receiveResult = null;

                    while (!_resetting)
                    {
                        _receiveResult = ClientSocket.ReceiveAsync(_buffer, default).Result;

                        if (_receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            TheLog.SocketLog?.Debug(LEFT_BRACKET + Request.Id + SOCKET_CLOSE_MESSAGE);
                            await ReconnectSocketAsync().ConfigureAwait(false);
                            break;
                        }

                        if (_receiveResult.EndOfMessage && !_multiPartMessage)
                        {
                            ProcessMessage(Encoding.GetString(_buffer.Array, _buffer.Offset, _receiveResult.Count));
                            break;
                        }

                        if (!_receiveResult.EndOfMessage)
                        {
                            _memoryStream ??= new MemoryStream();
                            await _memoryStream.WriteAsync(_buffer.Array, _buffer.Offset, _receiveResult.Count).ConfigureAwait(false);
                            _multiPartMessage = true;
                        }
                        else
                        {
                            await _memoryStream!.WriteAsync(_buffer.Array, _buffer.Offset, _receiveResult.Count).ConfigureAwait(false);
                            ProcessMessage(Encoding.GetString(_memoryStream!.ToArray(), 0, (int)_memoryStream.Length));
                            _memoryStream.Dispose();
                            break;
                        }
                    }
                }

                digestLoopActive = false;
            }
            catch
            {
                if (_memoryStream != null)
                {
                    _memoryStream.Dispose();
                    _memoryStream = null;
                }

                if (_resetting)
                {
                    return;
                }

                digestLoopActive = false;
                await ReconnectSocketAsync().ConfigureAwait(false);
            }
        }

        protected private void ProcessMessage(string data)
        {
#if DEBUG
            SocketLog?.Trace($"Socket {Request.Id} received data: " + data);
#endif
            JToken? tokenData = JToken.Parse(data);

            if (tokenData == null)
            {
                return;
            }

            if (socketClient.MessageMatchesHandler(tokenData, Request))
            {
                if (MessageHandler != null)
                {
                    MessageHandler(tokenData);
                }
            }
            else
            {
                PendingRequest[] requests;

                lock (pendingRequests)
                {
                    requests = pendingRequests.ToArray();
                }

                foreach (var request in requests)
                {
                    if (request.CheckData(tokenData))
                    {
                        lock (pendingRequests)
                        {
                            pendingRequests.Remove(request);
                        }

                        return;
                    }
                    else
                    if (request.Completed)
                    {
                        lock (pendingRequests)
                        {
                            pendingRequests.Remove(request);
                        }
                    }
                }
#if DEBUG
                SocketLog?.Error($"Socket {Request.Id} Message not handled: " + tokenData.ToString());
#endif
            }
        }

        protected private async Task ConnectionAttemptLoopInternalAsync(bool connecting = false)
        {
            try
            {
                if (SocketConnectionStatus == ConnectionStatus.Connecting || SocketConnectionStatus == ConnectionStatus.Waiting)
                {
                    return;
                }

                while (true)
                {
                    if (!ShouldAttemptConnection)
                    {
                        await DisposeAsync().ConfigureAwait(false);
                        return;
                    }

                    await Task.Delay(1).ConfigureAwait(false);

                    bool completed = await ConnectionAttemptCompletedInternal(connecting).ConfigureAwait(false);
                    if (completed)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                SocketLog?.Error(ex);
            }
        }

        protected private async Task<bool> ConnectionAttemptCompletedInternal(bool connecting = false)
        {
            var ReconnectTry = 0;
            var ResubscribeTry = 0;

            // Connection
            while (ShouldAttemptConnection)
            {
                SocketOnStatusChanged(ConnectionStatus.Waiting);
                await Task.Delay(1984).ConfigureAwait(false);
                SocketOnStatusChanged(ConnectionStatus.Connecting);

                ReconnectTry++;
                NewWebSocketInternal(true);

                if (!await ConnectionAttemptInternalAsync().ConfigureAwait(false))
                {
                    if (ReconnectTry >= socketClient.MaxReconnectTries)
                    {
                        SocketLog?.Debug($"Socket {Request.Id} failed to {(connecting ? "connect" : "reconnect")} after {ReconnectTry} tries, closing");
                        await DisposeAsync().ConfigureAwait(false);
                        return true;
                    }
                    else
                    {
                        SocketLog?.Debug($"[{DateTime.UtcNow - DisconnectTime}] Socket [{Request.Id}]  Failed to {(connecting ? "Connect" : "Reconnect")} - Attempts: {ReconnectTry}/{socketClient.MaxReconnectTries}");
                    }
                }
                else
                {
                    SocketLog?.Info($"[{DateTime.UtcNow - DisconnectTime}] Socket [{Request.Id}] {(connecting ? "Connected" : "Reconnected")} - Attempts: {ReconnectTry}/{socketClient.MaxReconnectTries}");
                    break;
                }
            }

            // Subscription
            while (ShouldAttemptConnection)
            {
                ResubscribeTry++;
                var reconnectResult = await ResubscriptionInternalAsync().ConfigureAwait(false);
                if (!reconnectResult)
                {
                    if (ResubscribeTry >= socketClient.MaxReconnectTries)
                    {
                        SocketLog?.Debug($"Socket {Request.Id} failed to {(connecting ? "subscribe" : "resubscribe")} after {ResubscribeTry} tries, closing");
                        await DisposeAsync().ConfigureAwait(false);
                        return true;
                    }
                    else
                    {
                        SocketLog?.Debug($"Socket {Request.Id}  {(connecting ? "subscribing" : "resubscribing")} subscription on  {(connecting ? "connected" : "reconnected")} socket{(socketClient.MaxReconnectTries != null ? $", try {ResubscribeTry}/{socketClient.MaxReconnectTries}" : "")}..");
                    }

                    if (!IsOpen)
                    {
                        // Disconnected while resubscribing
                        return false;
                    }
                }
                else
                {
                    ReconnectTry = 0;
                    ResubscribeTry = 0;
                    _ = Task.Run(() => ConnectionRestored?.Invoke(DisconnectTime.HasValue ? DateTime.UtcNow - DisconnectTime.Value : TimeSpan.FromSeconds(0))).ConfigureAwait(false);
                    return true;
                }
            }

            if (!ShouldAttemptConnection)
            {
                await DisposeAsync().ConfigureAwait(false);
            }

            return true;
        }

        /// <summary>
        /// Attempt to Connect and Start the WebSocket
        /// </summary>
        /// <returns></returns>
        protected private async Task<bool> ConnectionAttemptInternalAsync()
        {
#if DEBUG
            SocketLog?.Debug($"Socket {Request.Id} connecting..");
#endif
            try
            {
                sendLoopActive = false;
                digestLoopActive = false;

                await ClientSocket.ConnectAsync(UriClient.GetStream(), default).ConfigureAwait(false);
#if DEBUG
                SocketLog?.Trace($"Socket {Request.Id} connection succeeded, starting communication..");
#endif
                //  _sendTask = Task.Factory.StartNew(SendLoopAsync, default, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();
                // _receiveTask = Task.Factory.StartNew(DigestLoop, default, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();

                _ = Task.Run(SendLoopAsync).ConfigureAwait(false);
                _ = Task.Run(DigestLoopAsync).ConfigureAwait(false);

                bool b = await WaitForTasksSimple(true);
                if (b)
                {
#if DEBUG
                    SocketLog?.Debug($"Socket {Request.Id} connected..");
#endif
                    SocketOnStatusChanged(ConnectionStatus.Connected);
                    SocketLog?.Debug($"Socket {Request.Id} connected to {Url}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
#if DEBUG
            (Exception e)
#endif
            {
#if DEBUG
                SocketLog?.Debug($"Socket {Request.Id} connection failed: " + e.Message);
#endif
                SocketOnStatusChanged(ConnectionStatus.Error);
                return false;
            }
        }

        /// <summary>
        /// Create the WebSocket
        /// </summary>
        protected private void NewWebSocketInternal(bool reset = false)
        {
            if (reset)
            {
                TheLog.SocketLog?.Debug($"Socket {Request.Id} resetting..");
            }

            _sendEvent = new AsyncResetEvent();
            _sendBuffer = new ConcurrentQueue<byte[]>();

            ClientSocket = new ClientWebSocket();
            ClientSocket.Options.KeepAliveInterval = TimeSpan.FromMinutes(1);
            ClientSocket.Options.SetBuffer(8192, 8192);

            _resetting = false;
        }

        /// <summary>
        /// Connect the Socket
        /// </summary>
        /// <returns></returns>
        protected internal Task InternalConnectSocketAsync()
        {
            if (SocketConnectionStatus == ConnectionStatus.Connecting || SocketConnectionStatus == ConnectionStatus.Waiting || _resetting)
            {
                return Task.CompletedTask;
            }

            _resetting = true;

            TheLog.SocketLog?.Debug($"Connecting Socket {Request.Id}..");
            _ = Task.Run(async () =>
            {
                await ConnectionAttemptLoopInternalAsync(true).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Internal reset method, Will prepare the socket to be reset so it can be automatically reconnected or closed permanantly
        /// </summary>
        /// <returns></returns>
        public async Task ReconnectSocketAsync(bool connectAttempt = false)
        {
            try
            {
                if (SocketConnectionStatus == ConnectionStatus.Connecting || SocketConnectionStatus == ConnectionStatus.Waiting || _resetting)
                {
                    return;
                }

                _resetting = true;
                _sendEvent.Set();

                if (ClientSocket.State == WebSocketState.Open)
                {
                    await ClientSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, default).ConfigureAwait(false);
                    TheLog.SocketLog?.Debug($"Socket {Request.Id} has closed..");
                }
                else
                {
                    TheLog.SocketLog?.Debug($"Socket {Request.Id} was already closed..");
                }

                SocketOnStatusChanged(ConnectionStatus.Disconnected);

                lock (pendingRequests)
                {
                    foreach (var pendingRequest in pendingRequests.ToList())
                    {
                        pendingRequest.Fail();
                        pendingRequests.Remove(pendingRequest);
                    }
                }

                if (ShouldAttemptConnection)
                {
                    if (SocketConnectionStatus == ConnectionStatus.Connecting || SocketConnectionStatus == ConnectionStatus.Waiting)
                    {
                        return; // Already reconnecting
                    }

                    DisconnectTime = DateTime.UtcNow;
                    SocketOnStatusChanged(ConnectionStatus.Lost);
                    SocketLog?.Info($"Socket {Request.Id} Connection lost, will try to reconnect after 2000ms");

                    _ = Task.Run(async () =>
                    {
                        await ConnectionAttemptLoopInternalAsync().ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
                else
                {
                    SocketOnStatusChanged(ConnectionStatus.Closed);
                    SocketLog?.Info($"Socket {Request.Id} closed");
                    ClientSocket?.Dispose();
                }
            }
            catch (Exception ex)
            {
                SocketLog?.Error(ex);
                DisconnectTime = DateTime.UtcNow;
                SocketOnStatusChanged(ConnectionStatus.Lost);
                await ConnectionAttemptLoopInternalAsync().ConfigureAwait(false);
            }
        }

        protected private async Task<bool> ResubscriptionInternalAsync()
        {
            if (!IsOpen)
            {
                return false;
            }

            var task = await socketClient.SubscribeAndWaitAsync(this, Request).ConfigureAwait(false);

            if (!task.Success || !IsOpen)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Wait for Send/Digest loop to start/stop
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        protected private async Task<bool> WaitForTasksSimple(bool start)
        {
            var ticks = DateTime.Now.Ticks;
            while (true)
            {
                await Task.Delay(1).ConfigureAwait(false);

                if (start)
                {
                    if (sendLoopActive && digestLoopActive)
                    {
                        return true;
                    }
                }
                else
                {
                    if (!sendLoopActive && !digestLoopActive)
                    {
                        return true;
                    }
                }

                if (ticks + 5_000_000 > DateTime.Now.Ticks)
                {
                    return false;
                }
            }
        }

        protected private void SocketOnStatusChanged(ConnectionStatus obj)
        {
            SocketConnectionStatus = obj;
            ConnectionStatusChanged?.Invoke(obj);
        }
    }
}
