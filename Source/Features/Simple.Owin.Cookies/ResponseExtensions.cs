using System.Collections.Generic;
using System.Linq;

namespace Simple.Owin.Cookies
{
    internal static class ResponseExtensions
    {
        public static IEnumerable<HttpCookie> GetCookies(this IRequest request) {
            return request.Headers.Enumerate(HttpHeaderKeys.Cookie)
                          .Select(HttpCookie.Parse);
        }

        public static void SetCookie(this IResponse response, HttpCookie cookie) {
            response.Headers.Add(HttpHeaderKeys.SetCookie, cookie.ToResponseHeaderString());
        }
    }
}