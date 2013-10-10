using System;
using System.Collections.Generic;
using System.IO;

using Simple.Owin.Extensions;
using Simple.Owin.Http;

namespace Simple.Owin
{
    public class OwinResponse : IResponse
    {
        private readonly IDictionary<string, object> _environment;
        private readonly OwinResponseHeaders _headers;

        public OwinResponse(IDictionary<string, object> environment) {
            if (environment == null) {
                throw new ArgumentNullException("environment");
            }
            _environment = environment;
            var headers = _environment.GetValueOrDefault<IDictionary<string, string[]>>(OwinKeys.Response.Headers);
            if (headers == null) {
                headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                _environment.Add(OwinKeys.Response.Headers, headers);
            }
            _headers = new OwinResponseHeaders(headers);
        }

        public OwinResponseHeaders Headers {
            get { return _headers; }
        }

        public Stream Output {
            get { return _environment.GetValue<Stream>(OwinKeys.Response.Body); }
            set { _environment.SetValue(OwinKeys.Response.Body, value); }
        }

        public Status Status {
            get {
                //pull
                var status = _environment.GetValueOrDefault<Status>(OwinKeys.Simple.Status);
                if (status != null) {
                    return status;
                }
                //build
                var code = _environment.GetValueOrDefault(OwinKeys.Response.StatusCode, 0);
                if (code != 0) {
                    return new Status(code, _environment.GetValueOrDefault(OwinKeys.Response.ReasonPhrase, string.Empty));
                }
                //default
                return Status.Is.OK;
            }
            set {
                if (value == null) {
                    _environment.Remove(OwinKeys.Response.StatusCode);
                    _environment.Remove(OwinKeys.Response.ReasonPhrase);
                }
                else {
                    _environment.SetValue(OwinKeys.Response.StatusCode, value.Code);
                    _environment.SetValue(OwinKeys.Response.ReasonPhrase, value.Description);
                    if (value.LocationHeader != null) {
                        _headers.Location = value.LocationHeader;
                    }
                }
            }
        }

        public void DisableCaching() {
            _headers.SetValue(HttpHeaderKeys.CacheControl, "no-cache; no-store");
        }

        IResponseHeaders IResponse.Headers {
            get { return _headers; }
        }
    }
}