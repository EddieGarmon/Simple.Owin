using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Simple.Owin.Extensions;

namespace Simple.Owin.Support
{
    public class OwinContextBuilder
    {
        private readonly IDictionary<string, object> _environment;
        private readonly IDictionary<string, string[]> _requestHeaders;

        public OwinContextBuilder(IDictionary<string, object> environment) {
            _environment = environment;
            if (!_environment.ContainsKey(OwinKeys.Owin.CallCancelled)) {
                _environment.Add(OwinKeys.Owin.CallCancelled, new CancellationToken());
            }
            _requestHeaders = _environment.GetValueOrDefault<IDictionary<string, string[]>>(OwinKeys.Request.Headers);
            if (_requestHeaders == null) {
                _requestHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                _environment.Add(OwinKeys.Request.Headers, _requestHeaders);
            }
            if (!_environment.ContainsKey(OwinKeys.Response.Headers)) {
                _environment.Add(OwinKeys.Response.Headers, new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase));
            }
            // todo: what other items should we auto-populate if they dont exist?
        }

        public void AddRequestHeader(string key, string value) {
            _requestHeaders.AddValue(key, value);
        }

        public CancellationToken GetCancellationToken() {
            return _environment.GetValue<CancellationToken>(OwinKeys.Owin.CallCancelled);
        }

        public bool HasRequestHeader(string key, string value, bool shouldBeSingleValue) {
            string[] values;
            if (!_requestHeaders.TryGetValue(key, out values)) {
                return false;
            }
            if (shouldBeSingleValue) {
                return values.Length == 1 && values[0].Equals(value, StringComparison.OrdinalIgnoreCase);
            }
            //todo handle comma seperated list
            foreach (var existing in values) {
                if (existing.Equals(value, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        public void PrepareResponseHeaders() {
            _environment.SetValue(OwinKeys.Response.Headers, new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase));
        }

        public void SetFullUri(Uri value) {
            _environment.SetValue(OwinKeys.Simple.FullUri, value);
            //todo: should we automatically set all child parts if we know the PathBase?
            var pathBase = _environment.GetValueOrDefault<string>(OwinKeys.Request.PathBase);
            if (pathBase == null) {
                return;
            }
            SetScheme(value.Scheme);
            //todo: trim pathBase from path
            SetPath(value.AbsolutePath);
            SetQueryString(value.Query);
        }

        public void SetHttpMethod(string value) {
            _environment.SetValue(OwinKeys.Request.Method, value);
        }

        public void SetPath(string value) {
            _environment.SetValue(OwinKeys.Request.Path, value);
        }

        public void SetPathBase(string value) {
            _environment.SetValue(OwinKeys.Request.PathBase, value);
        }

        public void SetProtocol(string value) {
            _environment.SetValue(OwinKeys.Request.Protocol, value);
        }

        public void SetQueryString(string value) {
            _environment.SetValue(OwinKeys.Request.QueryString, value);
        }

        public void SetRequestStream(Stream value) {
            _environment.SetValue(OwinKeys.Request.Body, value);
        }

        public void SetResponseStream(Stream value) {
            _environment.SetValue(OwinKeys.Response.Body, value);
        }

        public void SetScheme(string value) {
            _environment.SetValue(OwinKeys.Request.Scheme, value);
        }
    }
}