using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;

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
        /// Gets the URL.
        /// </summary>
        Uri FullUri { get; }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        IRequestHeaders Headers { get; }

        /// <summary>
        /// Gets the input stream.
        /// </summary>
        Stream Input { get; }

        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        string Method { get; }

        string Path { get; }

        string PathBase { get; }

        string Protocol { get; }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        QueryString QueryString { get; }

        string Scheme { get; }

        IPrincipal User { get; set; }

        FormData FormData { get; set; }

        IEnumerable<HttpCookie> GetCookies();
    }
}