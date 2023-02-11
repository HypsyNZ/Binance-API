﻿/*
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
    /// Status of a fiat payment
    /// </summary>
    public enum FiatWithdrawDepositStatus
    {
        /// <summary>
        /// Still processing
        /// </summary>
        Processing = 0,

        /// <summary>
        /// Successfully completed
        /// </summary>
        Successful = 1,

        /// <summary>
        /// Failed
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Finished
        /// </summary>
        Finished = 3,

        /// <summary>
        /// Refunding
        /// </summary>
        Refunding = 4,

        /// <summary>
        /// Refunded
        /// </summary>
        Refunded = 5,

        /// <summary>
        /// Refund failed
        /// </summary>
        RefundFailed = 6,

        /// <summary>
        /// Expired
        /// </summary>
        Expired = 7
    }
}