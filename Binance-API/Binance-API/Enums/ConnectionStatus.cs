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

namespace BinanceAPI.Enums
{
    /// <summary>
    /// The current Sockets connection status
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Disconnected
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// Currently Connecting
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// Currently Connected
        /// </summary>
        Connected = 2,

        /// <summary>
        /// Connection was Lost
        /// </summary>
        Lost = 3,

        /// <summary>
        /// Connection was Closed
        /// </summary>
        Closed = 4,

        /// <summary>
        /// Connection encountered an Error
        /// </summary>
        Error = 5,

        /// <summary>
        /// Waiting for reconnect attempt
        /// </summary>
        Waiting = 6,

        /// <summary>
        /// Unsubscribed and Waiting
        /// </summary>
        Unsubscribed = 7
    }
}
