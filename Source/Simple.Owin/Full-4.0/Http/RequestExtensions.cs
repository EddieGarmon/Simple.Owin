namespace Simple.Web.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using Simple.Owin;

    /// <summary>
    /// Extension methods for the <see cref="IRequest"/> interface.
    /// </summary>
    public static class RequestExtensions
    {
        /// <summary>
        /// Tries to get the value of a Cookie.
        /// </summary>
        /// <param name="request">The <see cref="IRequest"/> instance.</param>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="value">The value of the cookie if found; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the cookie is found in the request; otherwise, <c>false</c>.</returns>
        public static bool TryGetCookieValue(this IRequest request, string name, out string value)
        {
            string[] cookies;
            if (request.Headers != null && request.Headers.TryGetValue(HeaderKeys.Cookie, out cookies))
            {
                cookies = cookies.SelectMany(c => c.Split(';').Select(s => s.Trim())).ToArray();
                var cookie = cookies.FirstOrDefault(c => c.StartsWith(name + "=", StringComparison.InvariantCultureIgnoreCase));
                if (cookie != null)
                {
                    value = GetCookieValue(cookie);
                    return true;
                }
            }
            value = null;
            return false;
        }

        internal static string GetCookieValue(string cookie)
        {
            int from = cookie.IndexOf('=');
            return HttpUtility.UrlDecode(cookie.Substring(from + 1));
        }
    }
}