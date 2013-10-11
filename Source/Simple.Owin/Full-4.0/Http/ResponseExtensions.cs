namespace Simple.Web.Http
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using Simple.Owin;
    using Simple.Web.Cors;

    /// <summary>
    /// Extension methods for the <see cref="IResponse"/> interface.
    /// </summary>
    public static class ResponseExtensions
    {
        private const int MinusOneYear = -31557600;
        private static readonly Regex CharsetCheck = new Regex(@";\s*charset=");
        private static readonly string[] MediaTypeWildcard = { "*/*" };



        public static void EnsureContentTypeCharset(this IResponse response, string charset = "utf-8")
        {
            string value;
            if (response.TryGetHeader(HeaderKeys.ContentType, out value))
            {
                if (!CharsetCheck.IsMatch(value))
                {
                    response.SetHeader(HeaderKeys.ContentType, value + "; charset=" + charset);
                }
            }
        }


        /// <summary>
        /// Sets the Access-Control-* headers.
        /// </summary>
        /// <param name="response">The <see cref="IResponse"/> instance.</param>
        /// <param name="accessControl">A <see cref="IAccessControlEntry"/> containing the header values.</param>
        public static void SetAccessControl(this IResponse response, IAccessControlEntry accessControl)
        {
            response.SetHeader(HeaderKeys.AccessControlAllowOrigin, accessControl.Origin);
            if (!string.IsNullOrWhiteSpace(accessControl.AllowHeaders))
            {
                response.SetHeader(HeaderKeys.AccessControlAllowHeaders, accessControl.AllowHeaders);
            }
            if (!string.IsNullOrWhiteSpace(accessControl.ExposeHeaders))
            {
                response.SetHeader(HeaderKeys.AccessControlExposeHeaders, accessControl.ExposeHeaders);
            }
            if (!string.IsNullOrWhiteSpace(accessControl.Methods))
            {
                response.SetHeader(HeaderKeys.AccessControlAllowMethods, accessControl.Methods);
            }
            if (accessControl.Credentials.HasValue)
            {
                response.SetHeader(HeaderKeys.AccessControlAllowCredentials, accessControl.Credentials.Value.ToString());
            }
            if (accessControl.MaxAge.HasValue)
            {
                response.SetHeader(HeaderKeys.AccessControlMaxAge, accessControl.MaxAge.Value.ToString(CultureInfo.InvariantCulture));
            }
        }


    }
}