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

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BinanceAPI.Requests
{
    /// <summary>
    /// HttpWebResponse response object
    /// </summary>
    public class Response
    {
        private readonly HttpResponseMessage response;

        /// <inheritdoc />
        public HttpStatusCode StatusCode => response.StatusCode;

        /// <inheritdoc />
        public bool IsSuccessStatusCode => response.IsSuccessStatusCode;

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ResponseHeaders => response.Headers;

        /// <summary>
        /// Create response for a http response message
        /// </summary>
        /// <param name="response">The actual response</param>
        public Response(HttpResponseMessage response)
        {
            this.response = response;
        }

        /// <inheritdoc />
        public async Task<Stream> GetResponseStreamAsync()
        {
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Close()
        {
            response.Dispose();
        }
    }
}
