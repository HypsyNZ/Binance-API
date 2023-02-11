using BinanceAPI.ClientBase;

namespace BinanceAPI.Options
{
    /// <summary>
    /// Base client options
    /// </summary>
    public class ClientOptions : BaseOptions
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, Credentials: {(!BaseClient.IsAuthenticationSet ? "Not Set" : "Set")}, BaseAddress: {UriClient.GetBaseAddress()}";
        }
    }
}