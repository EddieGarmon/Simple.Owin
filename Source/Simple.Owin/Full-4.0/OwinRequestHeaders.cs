using System.Collections.Generic;

using Simple.Owin.Http;

namespace Simple.Owin
{
    public class OwinRequestHeaders : OwinHeaders, IRequestHeaders
    {
        public OwinRequestHeaders(IDictionary<string, string[]> raw)
            : base(raw) { }

        public string Accept {
            get { return GetValue(HttpHeaderKeys.Accept) ?? "*/*"; }
            set { SetValue(HttpHeaderKeys.Accept, value); }
        }

        public string ContentType {
            get { return GetValue(HttpHeaderKeys.ContentType); }
            set { SetValue(HttpHeaderKeys.ContentType, value); }
        }

        public string Host {
            get { return GetValue(HttpHeaderKeys.Host); }
            set { SetValue(HttpHeaderKeys.Host, value); }
        }
    }
}