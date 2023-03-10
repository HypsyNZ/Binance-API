/*
*MIT License
*
*Copyright (c) 2022 S Christison
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

using BinanceAPI;
using BinanceAPI.ClientBase;
using BinanceAPI.ClientHosts;
using BinanceAPI.Enums;
using System;
using System.Threading.Tasks;

namespace API_Test
{
    // SOCKET TEST
    internal class SocketTest
    {
        public static void Run(SocketClientHost socketClient)
        {
            _ = Task.Run(() =>
            {
                //var _breakpoint = socketClient;
                //_ = _breakpoint;

                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000).ConfigureAwait(false);

                    // Create Update Subscription
                    BaseSocketClient sub = socketClient.Spot.SubscribeToBookTickerUpdatesAsync("BTCUSDT", data =>
                    {
                        // Uncomment to see output from the Socket
                        Console.WriteLine("[" + data.Data.UpdateId +
                            "]| BestAsk: " + data.Data.BestAskPrice.Normalize().ToString("0.00") +
                            " | Ask Quan: " + data.Data.BestAskQuantity.Normalize().ToString("000.00000#####") +
                            " | BestBid :" + data.Data.BestBidPrice.Normalize().ToString("0.00") +
                            " | BidQuantity :" + data.Data.BestBidQuantity.Normalize().ToString("000.00000#####"));
                    }).Result.Data;

                    // Subscribe to Update Subscription Status Changed Events before it connects
                    sub.ConnectionStatusChanged += BinanceSocket_StatusChanged;

                    // Current Status
                    Console.WriteLine(Enum.GetName(typeof(ConnectionStatus), sub.SocketConnectionStatus));

                    // work work
                    await Task.Delay(5000).ConfigureAwait(false);

                    // Reconnect Update Subscription
                    await sub.ReconnectSocketAsync().ConfigureAwait(false);

                    // work work
                    await Task.Delay(5000).ConfigureAwait(false);

                    // Reconnect Update Subscription
                    await sub.ReconnectSocketAsync().ConfigureAwait(false);

                    // work work
                    await Task.Delay(5000).ConfigureAwait(false);

                    // Unsubscribe
                    bool b = await sub.UnsubscribeAsync().ConfigureAwait(false);
                    if (b)
                    {
                        Console.WriteLine("Unsubscribed Successfully");
                    }

                    // wait
                    await Task.Delay(5000).ConfigureAwait(false);

                    // Resubscribe
                    bool b2 = await sub.ResubscribeAsync().ConfigureAwait(false);
                    if (b2)
                    {
                        Console.WriteLine("Resubscribed Successfully");
                    }

                    // wait
                    await Task.Delay(5000).ConfigureAwait(false);

                    // Destroy everything and unsubscribe
                    bool b3 = await sub.DisposeAsync().ConfigureAwait(false);
                    if (b3)
                    {
                        Console.WriteLine("Disposed Successfully");
                    }

                    // wait
                    await Task.Delay(5000).ConfigureAwait(false);

                    sub = null;
                    Console.WriteLine("Create New Socket");

                    sub = socketClient.Spot.SubscribeToBookTickerUpdatesAsync("BTCUSDT", data =>
                    {
                        // Uncomment to see output from the Socket
                        Console.WriteLine("[" + data.Data.UpdateId +
                            "]| BestAsk: " + data.Data.BestAskPrice.Normalize().ToString("0.00") +
                            " | Ask Quan: " + data.Data.BestAskQuantity.Normalize().ToString("000.00000#####") +
                            " | BestBid :" + data.Data.BestBidPrice.Normalize().ToString("0.00") +
                            " | BidQuantity :" + data.Data.BestBidQuantity.Normalize().ToString("000.00000#####"));
                    }).Result.Data;

                    await Task.Delay(5000).ConfigureAwait(false);

                    // Destroy everything and unsubscribe
                    bool b4 = await sub.DisposeAsync().ConfigureAwait(false);
                    if (b4)
                    {
                        Console.WriteLine("Disposed Successfully");
                    }

                    Console.WriteLine("Completed");

                    //// TEST BEGINS
                    //for (int i = 0; i < 1000; i++)
                    //{
                    //    await Task.Delay(1).ConfigureAwait(false);

                    //    // Last Subscription Socket Action Time In Ticks
                    //    Console.WriteLine(sub.Connection.Socket.LastActionTime.Ticks);
                    //}
                }).ConfigureAwait(false);
            });
        }

        private static void BinanceSocket_StatusChanged(ConnectionStatus obj)
        {
            Console.WriteLine("Status: " + obj.ToString());
        }

    }
}
