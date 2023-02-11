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

using BinanceAPI.Converters;
using BinanceAPI.Enums;

using Newtonsoft.Json;
using System;

namespace BinanceAPI.Objects.Spot.MarginData
{
    /// <summary>
    /// Transfer history entry
    /// </summary>
    public class BinanceTransferHistory
    {
        /// <summary>
        /// Amount of the transfer
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Asset of the transfer
        /// </summary>
        public string Asset { get; set; } = string.Empty;

        /// <summary>
        /// Status of the transfer
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the transaction
        /// </summary>
        [JsonConverter(typeof(TimestampConverter))]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Transaction id
        /// </summary>
        [JsonProperty("txId")]
        public decimal TransactionId { get; set; }

        /// <summary>
        /// Direction of the transfer
        /// </summary>
        [JsonProperty("type"), JsonConverter(typeof(TransferDirectionConverter))]
        public TransferDirection Direction { get; set; }
    }
}
