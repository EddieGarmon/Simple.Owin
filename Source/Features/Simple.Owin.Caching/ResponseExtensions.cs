//using Simple.Owin;

namespace Simple.Owin.Caching
{
    internal static class ResponseExtensions
    {
        /// <summary>
        /// Sets the Cache-Control header and optionally the Expires and Vary headers.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cacheOptions">A <see cref="CacheOptions"/> object to specify the cache settings.</param>
        public static void SetCacheOptions(this IResponse response, CacheOptions cacheOptions) {
            if (cacheOptions == null) {
                return;
            }
            if (cacheOptions.Disable) {
                response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                response.Headers.Pragma = "no-cache";
                response.Headers.Expires = "0";
                return;
            }
            response.Headers.CacheControl = cacheOptions.ToHeaderString();
            if (cacheOptions.AbsoluteExpiry.HasValue) {
                response.Headers.Expires = cacheOptions.AbsoluteExpiry.Value.ToString("R");
            }
            if (cacheOptions.VaryByHeaders != null && cacheOptions.VaryByHeaders.Count > 0) {
                response.Headers.Vary = string.Join(", ", cacheOptions.VaryByHeaders);
            }
        }
    }
}