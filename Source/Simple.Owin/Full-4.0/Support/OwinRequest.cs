using System;
using System.Collections.Generic;
using System.IO;

using Simple.Owin.Extensions;

namespace Simple.Owin.Support
{
    internal class OwinRequest : IRequest
    {
        private readonly IDictionary<string, object> _environment;
        private IRequestHeaders _headers;
        private IDictionary<string, string[]> _queryString;

        public OwinRequest(IDictionary<string, object> environment) {
            _environment = environment;
        }

        public IEnumerable<IPostedFile> Files {
            get {
                throw new NotImplementedException();
                // this is using ASP.NET.HttpContext:
                //if (_context != null)
                //{
                //    for (int i = 0; i < _context.Request.Files.Count; i++)
                //    {
                //        yield return new PostedFile(_context.Request.Files.Get(i));
                //    }
                //}
            }
        }

        public IRequestHeaders Headers {
            get {
                return _headers ??
                       (_headers = new OwinRequestHeaders(_environment.GetValue<IDictionary<string, string[]>>(OwinKeys.Request.Headers)));
            }
        }

        public string HttpMethod {
            get { return _environment.GetValue<string>(OwinKeys.Request.Method); }
        }

        public Stream InputStream {
            get { return _environment.GetValue<Stream>(OwinKeys.Request.Body); }
        }

        public IDictionary<string, string[]> QueryString {
            get {
                if (_queryString == null) {
                    var queryString = _environment.GetValueOrDefault(OwinKeys.Request.QueryString, string.Empty);
                    _queryString = QueryStringParser.Parse(queryString);
                }
                return _queryString;
            }
        }

        public Uri Url {
            get { return _environment.GetValueOrCreate(OwinKeys.Simple.FullUri, MakeUri); }
        }

        private Uri MakeUri() {
            var scheme = _environment.GetValueOrDefault(OwinKeys.Request.Scheme, "http");
            string host = Headers.Host ?? // should be here for http 1.1 requests
                          _environment.GetValueOrDefault<string>(OwinKeys.Server.LocalIpAddress) ?? // add port
                          "localhost"; // last resort
            int port = _environment.GetValueOrDefault(OwinKeys.Server.LocalPort, 80);
            var pathBase = _environment.GetValueOrDefault(OwinKeys.Request.PathBase, string.Empty);
            var path = _environment.GetValueOrDefault(OwinKeys.Request.Path, "/");
            var queryString = _environment.GetValueOrDefault(OwinKeys.Request.QueryString, string.Empty);
            var builder = new UriBuilder(scheme, host, port, pathBase + path, queryString);
            return builder.Uri;
        }
    }
}