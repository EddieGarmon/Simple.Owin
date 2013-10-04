using System.Collections.Generic;

namespace Simple.Owin
{
    /// <summary>
    /// Represents the context for a request/response cycle.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Gets the request.
        /// </summary>
        IRequest Request { get; }

        /// <summary>
        /// Gets the response.
        /// </summary>
        IResponse Response { get; }

        /// <summary>
        /// Gets a general-purpose store for variables that can be used for storing stuff for the lifetime of the request.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        IDictionary<string, object> Environment { get; }
    }
}