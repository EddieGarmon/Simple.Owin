using System.Collections.Generic;
using System.IO;

namespace Simple.Owin
{
    /// <summary>
    /// Abstraction for an HTTP response, to be implemented by hosting.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// The response headers.
        /// </summary>
        IDictionary<string, string[]> Headers { get; }

        Stream Output { get; }

        /// <summary>
        /// Gets or sets the status code and description.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        Status Status { get; set; }
    }
}