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

using BinanceAPI.ClientHosts;
using BinanceAPI.Enums;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace API_Test
{
    // EXCHANGE INFO
    internal class ExchangeInfoTest
    {
        public static void Run(BinanceClientHost client)
        {
            _ = Task.Run(async () =>
            {
                var result = await client.Spot.System.GetExchangeInfoAsync().ConfigureAwait(false);

                if (result.Success)
                {
                    // All Exchange Symbols
                    Console.WriteLine("Passed loaded exchange info for: [" + result.Data.Symbols.Count() + "] symbols");

                    // Rate Limits
                    Console.WriteLine("Rate: " + result.Data.RateLimits.First().Interval.ToString() + " | Limit: " + result.Data.RateLimits.First().Limit.ToString());

                    // Permissions
                    var r = result.Data.Symbols.Where(t => t.Permissions != null).FirstOrDefault();
                    Console.WriteLine(r.Name);
                    foreach (var p in r.Permissions)
                    {
                        Console.WriteLine(p);
                    }

                    // Max Position
                    var max = result.Data.Symbols.Where(t => t.MaxPositionFilter != null).FirstOrDefault();
                    if (max != null)
                    {
                        Console.WriteLine("Symbol: " + max.Name + " | Max Possible Position: " + max.MaxPositionFilter.MaxPosition);
                    }

                    Console.WriteLine("Spot: " + AccountType.Spot.ToString());
                    Console.WriteLine("Spot: " + nameof(AccountType.Spot));

                    // Convert to Json
                    var se = JsonConvert.SerializeObject(result.Data);

                    //_ = true; // Breakpoint;

                    // Convert from Json
                    var de = JsonConvert.DeserializeObject(se);

                    Trace.WriteLine(de);
                }

                var results = await client.Spot.System.GetExchangeInfoAsync(AccountType.Spot).ConfigureAwait(false);
                if (results.Success)
                {
                    // Spot
                    Console.WriteLine("Spot symbols: [" + results.Data.Symbols.Count() + "]");
                }
                else
                {
                    Console.WriteLine(results.Error.Message);
                    Trace.WriteLine(results.Error.Message);
                }

                var resultm = await client.Spot.System.GetExchangeInfoAsync(AccountType.Margin).ConfigureAwait(false);
                if (resultm.Success)
                {
                    // Margin
                    Console.WriteLine("Margin symbols: [" + resultm.Data.Symbols.Count() + "]");
                }
                else
                {
                    Console.WriteLine(resultm.Error.Message);
                    Trace.WriteLine(resultm.Error.Message);
                }

                var resultl = await client.Spot.System.GetExchangeInfoAsync(AccountType.Leveraged).ConfigureAwait(false);
                if (resultl.Success)
                {
                    // Leveraged
                    Console.WriteLine("Leveraged symbols: [" + resultl.Data.Symbols.Count() + "]");
                }
                else
                {
                    Console.WriteLine(resultl.Error.Message);
                    Trace.WriteLine(resultl.Error.Message);
                }

                string[] all = { "SPOT", "MARGIN", "LEVERAGED", "TRD_GRP_002", "TRD_GRP_003", "TRD_GRP_004", "TRD_GRP_005" };
                var resultall = await client.Spot.System.GetExchangeInfoAsync(all, true).ConfigureAwait(false);
                if (resultall.Success)
                {
                    // All Permissions Test
                    Console.WriteLine("All symbols: [" + resultall.Data.Symbols.Count() + "]");
                }
                else
                {
                    Console.WriteLine(resultall.Error.Message);
                    Trace.WriteLine(resultall.Error.Message);
                }

                AccountType[] all2 = { AccountType.Margin, AccountType.Leveraged };
                var resultall2 = await client.Spot.System.GetExchangeInfoAsync(all2).ConfigureAwait(false);
                if (resultall2.Success)
                {
                    // Margin/Leveraged
                    Console.WriteLine("Margin/Leveraged: [" + resultall2.Data.Symbols.Count() + "]");
                }
                else
                {
                    Console.WriteLine(resultall2.Error.Message);
                    Trace.WriteLine(resultall2.Error.Message);
                }

            }).ConfigureAwait(false);
        }
    }
}
