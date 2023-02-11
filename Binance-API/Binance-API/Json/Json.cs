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

using BinanceAPI.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static BinanceAPI.TheLog;

namespace BinanceAPI
{
    /// <summary>
    /// Serialize / Deserialize Json
    /// </summary>
    public class Json
    {
        /// <summary>
        /// The Serializer
        /// </summary>
        public static JsonSerializer DefaultSerializer { get; set; }

        static Json()
        {
            DefaultSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Culture = CultureInfo.InvariantCulture
            });
        }

        internal static CallResult<JToken> ValidateJson(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                var info = "Empty data object received";
                ClientLog?.Error(info);
                return new CallResult<JToken>(null, new DeserializeError(info, data));
            }

            try
            {
                return new CallResult<JToken>(JToken.Parse(data), null);
            }
            catch (JsonReaderException) { return new CallResult<JToken>(null, null); }
        }

        /// <summary>
        /// Deserialize a JToken into an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="obj">The data to deserialize</param>
        /// <returns></returns>
        public static CallResult<T> Deserialize<T>(JToken obj)
        {
            try
            {
                return new CallResult<T>(obj.ToObject<T>(DefaultSerializer), null);
            }

            catch (JsonReaderException) { return new CallResult<T>(default, null); }
        }

        /// <summary>
        /// Deserialize a stream into an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="stream">The stream to deserialize</param>
        /// <returns></returns>
        public static Task<CallResult<T>> DeserializeAsync<T>(Stream stream)
        {
            try
            {
                // Let the reader keep the stream open so we're able to seek if needed. The calling method will close the stream.
                using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);

                using var jsonReader = new JsonTextReader(reader);

                return Task.FromResult(new CallResult<T>(DefaultSerializer.Deserialize<T>(jsonReader), null));

            }

            catch (JsonReaderException) { return Task.FromResult(new CallResult<T>(default, null)); }
        }
    }
}
