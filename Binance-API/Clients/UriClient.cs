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
using BinanceAPI.UriBase;
using System;

namespace BinanceAPI
{
    /// <summary>
    /// The Static Client that manages Uris for Requests
    /// This feature is a WIP
    /// </summary>
    public class UriClient
    {
        private static readonly UriHelper _allEndpoints;

        private static readonly Uri API_CONTROLLER_DEFAULT;
        private static readonly Uri API_CONTROLLER_ONE;
        private static readonly Uri API_CONTROLLER_TWO;
        private static readonly Uri API_CONTROLLER_THREE;
        private static readonly Uri API_CONTROLLER_FOUR;
        private static readonly Uri API_CONTROLLER_TEST;

        private static readonly Uri WSS_DEFAULT;
        private static readonly Uri WSS_ONE;
        private static readonly Uri WSS_TWO;
        private static readonly Uri WSS_THREE;
        private static readonly Uri WSS_FOUR;
        private static readonly Uri WSS_TEST;

        static UriClient()
        {
            _allEndpoints = new UriHelper();
            
            API_CONTROLLER_DEFAULT = new Uri(@"https://api.binance.com/");
            API_CONTROLLER_ONE = new Uri(@"https://api1.binance.com/");
            API_CONTROLLER_TWO = new Uri(@"https://api2.binance.com/");
            API_CONTROLLER_THREE = new Uri(@"https://api3.binance.com/");
            API_CONTROLLER_FOUR = new Uri(@"https://api4.binance.com");
            API_CONTROLLER_TEST = new Uri(@"https://testnet.binance.vision/");
            
            WSS_DEFAULT = new Uri(@"wss://stream.binance.com:9443/stream");
            WSS_ONE = new Uri(@"wss://stream1.binance.com:9443/stream");
            WSS_TWO = new Uri(@"wss://stream2.binance.com:9443/stream");
            WSS_THREE = new Uri(@"wss://stream3.binance.com:9443/stream");
            WSS_FOUR = new Uri(@"wss://stream4.binance.com:9443/stream");
            WSS_TEST = new Uri(@"wss://testnet.binance.vision/stream");
        }

        /// <summary>
        /// API Endpoint Uri Helper
        /// </summary>
        public static UriHelper GetEndpoint => _allEndpoints;

        /// <summary>
        /// Get the current Api Endpoint Base Address
        /// </summary>
        /// <returns></returns>
        public static Uri GetBaseAddress()
        {
            switch (BaseClient.ChangeEndpoint)
            {
                case ApiEndpoint.DEFAULT:
                    return API_CONTROLLER_DEFAULT;
                case ApiEndpoint.ONE:
                    return API_CONTROLLER_ONE;
                case ApiEndpoint.TWO:
                    return API_CONTROLLER_TWO;
                case ApiEndpoint.THREE:
                    return API_CONTROLLER_THREE;
                case ApiEndpoint.FOUR:
                    return API_CONTROLLER_FOUR;
                case ApiEndpoint.TEST:
                    return API_CONTROLLER_TEST;
                default:
                    return API_CONTROLLER_DEFAULT;
            }
        }

        /// <summary>
        /// Get the current Web Socket Stream for the Endpoint Controller that is currently selected
        /// </summary>
        /// <returns></returns>
        public static Uri GetStream()
        {
            switch (BaseClient.ChangeEndpoint)
            {
                case ApiEndpoint.DEFAULT:
                    return WSS_DEFAULT;
                case ApiEndpoint.ONE:
                    return WSS_ONE;
                case ApiEndpoint.TWO:
                    return WSS_TWO;
                case ApiEndpoint.THREE:
                    return WSS_THREE;
                case ApiEndpoint.FOUR:
                    return WSS_FOUR;
                case ApiEndpoint.TEST:
                    return WSS_TEST;
                default:
                    return WSS_DEFAULT;
            }
        }
    }
}
