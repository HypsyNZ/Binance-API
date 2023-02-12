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
using System;

namespace BinanceAPI
{
    /// <summary>
    /// The Log Message
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The DateTime this Message was logged
        /// </summary>
        public DateTime LoggedDateTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// The Message Content
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The Log Level for the Message
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// The Message
        /// </summary>
        /// <param name="message">The Message Content</param>
        /// <param name="dateFormat">The Date Format for the message</param>
        /// <param name="loglevel">The Log Level for the Message</param>
        public Message(string message, string dateFormat, LogLevel loglevel)
        {
            Level = loglevel;
            LoggedDateTime = DateTime.Now;
            Content = "[" + LoggedDateTime.ToString(dateFormat) + "]" + message;
        }
    }
}
