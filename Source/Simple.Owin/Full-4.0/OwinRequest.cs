using System;
using System.Collections.Generic;
using System.IO;

using Simple.Owin.Extensions;

namespace Simple.Owin
{
    public class OwinRequest : IRequest
    {
        private readonly IDictionary<string, object> _environment;
        private readonly OwinRequestHeaders _headers;
        private QueryString _queryString;

        public OwinRequest(IDictionary<string, object> environment) {
            if (environment == null) {
                throw new ArgumentNullException("environment");
            }
            _environment = environment;
            var headers = _environment.GetValueOrDefault<IDictionary<string, string[]>>(OwinKeys.Request.Headers);
            if (headers == null) {
                headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                _environment.Add(OwinKeys.Request.Headers, headers);
            }
            _headers = new OwinRequestHeaders(headers);
        }

        public IEnumerable<IPostedFile> Files {
            get { throw new NotImplementedException(); }
        }

        public Uri FullUri {
            get { return _environment.GetValueOrCreate(OwinKeys.Simple.FullUri, MakeUri); }
            set {
                _environment.SetValue(OwinKeys.Simple.FullUri, value);
                //todo: should we automatically set all child parts if we know the PathBase?
                var pathBase = _environment.GetValueOrDefault<string>(OwinKeys.Request.PathBase);
                if (pathBase == null) {
                    return;
                }
                Scheme = value.Scheme;
                //todo: trim pathBase from path
                Path = value.AbsolutePath;
                QueryString = value.Query;
            }
        }

        public OwinRequestHeaders Headers {
            get { return _headers; }
        }

        public Stream Input {
            get { return _environment.GetValue<Stream>(OwinKeys.Request.Body); }
            set { _environment.SetValue(OwinKeys.Request.Body, value); }
        }

        public string Method {
            get { return _environment.GetValue<string>(OwinKeys.Request.Method); }
            set { _environment.SetValue(OwinKeys.Request.Method, value); }
        }

        public string Path {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Request.Path); }
            set { _environment.SetValue(OwinKeys.Request.Path, value); }
        }

        public string PathBase {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Request.PathBase); }
            set { _environment.SetValue(OwinKeys.Request.PathBase, value); }
        }

        public string Protocol {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Request.Protocol); }
            set { _environment.SetValue(OwinKeys.Request.Protocol, value); }
        }

        public QueryString QueryString {
            get { return _queryString ?? (_queryString = _environment.GetValueOrDefault(OwinKeys.Request.QueryString, string.Empty)); }
            set {
                _queryString = value;
                _environment.SetValue(OwinKeys.Request.QueryString, value);
            }
        }

        public string Scheme {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Request.Scheme); }
            set { _environment.SetValue(OwinKeys.Request.Scheme, value); }
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

        IRequestHeaders IRequest.Headers {
            get { return _headers; }
        }
    }
}