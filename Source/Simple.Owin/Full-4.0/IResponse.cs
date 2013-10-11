using System.IO;

namespace Simple.Owin
{
    /// <summary>
    /// Abstraction for an HTTP response, to be implemented by hosting.
    /// </summary>
    public interface IResponse
    {
        Stream Body { get; }

        /// <summary>
        /// The response headers.
        /// </summary>
        IResponseHeaders Headers { get; }

        /// <summary>
        /// Gets or sets the status code and description.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        Status Status { get; set; }

        /// <summary>
        /// Sets the Cache-Control header and optionally the Expires and Vary headers.
        /// </summary>
        /// <param name="cacheOptions">A <see cref="CacheOptions"/> object to specify the cache settings.</param>
        void SetCacheOptions(CacheOptions cacheOptions);
    }
}