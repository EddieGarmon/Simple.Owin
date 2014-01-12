using System;
using System.Text;
using Simple.Owin.Helpers;

namespace Simple.Owin.Cookies
{
    internal class HttpCookie
    {
        private readonly string _name;
        private readonly string _value;

        public HttpCookie(string name, string value) {
            _name = name;
            _value = value;
        }

        public string Domain { get; set; }

        public bool HttpOnly { get; set; }

        public string Name {
            get { return _name; }
        }

        public string Path { get; set; }

        public bool Secure { get; set; }

        public TimeSpan TimeOut { get; set; }

        public string Value {
            get { return _value; }
        }

        public string ToRequestHeaderString() {
            return string.Format("{0}={1}", Name, UrlHelper.Encode(Value));
        }

        public string ToResponseHeaderString() {
            Path = Path ?? "/";
            var builder = new StringBuilder();
            builder.AppendFormat("{0}={1}", Name, UrlHelper.Encode(Value));
            if (!string.IsNullOrWhiteSpace(Domain)) {
                builder.AppendFormat("; Domain={0}", Domain);
            }
            builder.AppendFormat("; Path={0}", Path);
            if (TimeOut != TimeSpan.Zero) {
                builder.AppendFormat("; Expires={0}", (DateTime.UtcNow + TimeOut).ToString("R"));
            }

            if (Secure) {
                builder.Append("; Secure");
            }
            if (HttpOnly) {
                builder.Append("; HttpOnly");
            }

            return builder.ToString();
        }

        public static HttpCookie Parse(string value) {
            string[] avPairs = value.Split(';');
            string[] nameValue = avPairs[0].Split('=');
            var cookie = new HttpCookie(nameValue[0], nameValue[1]);
            for (int i = 1; i < avPairs.Length; i++) {
                var avPair = avPairs[i];
                string[] attribute = avPair.Split('=');
                switch (attribute[0].Trim()
                                    .ToLowerInvariant()) {
                                        case "domain":
                                            cookie.Domain = attribute[1].Trim();
                                            break;
                                        case "path":
                                            cookie.Path = attribute[1].Trim();
                                            break;
                                        case "expires":
                                            //todo
                                            break;
                                        case "secure":
                                            cookie.Secure = true;
                                            break;
                                        case "httponly":
                                            cookie.HttpOnly = true;
                                            break;
                }
            }
            return cookie;
        }
    }
}