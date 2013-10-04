using System;
using System.Collections.Generic;
using System.IO;

namespace Simple.Owin
{
    /// <summary>
    /// Abstraction for an HTTP request
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Gets the list of uploaded files.
        /// </summary>
        IEnumerable<IPostedFile> Files { get; }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        IRequestHeaders Headers { get; }

        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        string HttpMethod { get; }

        /// <summary>
        /// Gets the input stream.
        /// </summary>
        Stream InputStream { get; }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        IDictionary<string, string[]> QueryString { get; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        Uri Url { get; }
    }
}