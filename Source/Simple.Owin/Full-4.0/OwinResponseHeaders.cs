using System;
using System.Collections.Generic;
using System.Globalization;

using Simple.Owin.Http;

namespace Simple.Owin
{
    public class OwinResponseHeaders : OwinHeaders, IResponseHeaders
    {
        public OwinResponseHeaders(IDictionary<string, string[]> raw)
            : base(raw) { }

        public string Connection {
            get { return GetValue(HttpHeaderKeys.Connection); }
            set { SetValue(HttpHeaderKeys.Connection, value); }
        }

        public long ContentLength {
            get {
                string value = GetValue(HttpHeaderKeys.ContentLength);
                if (value == null) {
                    return -1;
                }
                return Convert.ToInt64(value);
            }
            set { SetValue(HttpHeaderKeys.ContentLength, value < 0 ? null : value.ToString(CultureInfo.InvariantCulture)); }
        }

        public string ContentType {
            get { return GetValue(HttpHeaderKeys.ContentType); }
            set { SetValue(HttpHeaderKeys.ContentType, value); }
        }

        public string Location {
            get { return GetValue(HttpHeaderKeys.Location); }
            set { SetValue(HttpHeaderKeys.Location, value); }
        }
    }
}