using System;

namespace Simple.Owin.Testing
{
    public class TestRequest
    {
        private readonly HttpHeaders _headers = new HttpHeaders(Make.Headers());
        private readonly HttpRequestLine _requestLine;
        private Uri _url;
        //todo body

        private TestRequest(HttpRequestLine requestLine) {
            _requestLine = requestLine;
        }

        internal HttpHeaders Headers {
            get { return _headers; }
        }

        internal HttpRequestLine RequestLine {
            get { return _requestLine; }
        }

        internal Uri Url {
            get { return _url ?? (_url = BuildUrl()); }
        }

        public TestRequest WithCookie(string name, string value) {
            _headers.Add(HttpHeaderKeys.Cookie, string.Format("{0}={1}", name, value));
            return this;
        }

        public TestRequest WithHeader(string name, string value) {
            _headers.Add(name, value);
            return this;
        }

        private Uri BuildUrl() {
            if (RequestLine.Uri.StartsWith("http")) {
                return new Uri(RequestLine.Uri, UriKind.Absolute);
            }
            if (RequestLine.Uri.StartsWith("/")) {
                string host = _headers.GetValue(HttpHeaderKeys.Host) ?? "localhost";
                return new Uri("http://" + host + RequestLine.Uri, UriKind.Absolute);
            }
            throw new Exception("invalid test url format.");
        }

        public static TestRequest Delete(string url) {
            return new TestRequest(new HttpRequestLine("DELETE", url, "HTTP/1.1"));
        }

        public static TestRequest Get(string url) {
            return new TestRequest(new HttpRequestLine("GET", url, "HTTP/1.1"));
        }

        public static TestRequest Post(string url) {
            return new TestRequest(new HttpRequestLine("POST", url, "HTTP/1.1"));
        }

        public static TestRequest Put(string url) {
            return new TestRequest(new HttpRequestLine("PUT", url, "HTTP/1.1"));
        }
    }
}