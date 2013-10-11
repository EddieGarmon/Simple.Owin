using System;
using System.Collections.Generic;
using System.IO;

using Simple.Owin.Extensions;

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
            var headers = _environment.GetValueOrCreate(OwinKeys.Response.Headers, Make.Headers);
            _headers = new OwinResponseHeaders(headers);
        }

        public Stream Body {
            get { return _environment.GetValue<Stream>(OwinKeys.Response.Body); }
            set { _environment.SetValue(OwinKeys.Response.Body, value); }
        }

        public OwinResponseHeaders Headers {
            get { return _headers; }
        }

        public string Protocol {
            get { return _environment.GetValueOrDefault<string>(OwinKeys.Response.Protocol); }
            set { _environment.SetValue(OwinKeys.Response.Protocol, value); }
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

        public void RemoveCookie(string cookieName) {
            _headers.Add(HttpHeaderKeys.SetCookie, string.Format("{0}=; Expires=Thu, 01 Jan 1970 00:00:00 GMT", cookieName));
        }

        /// <summary>
        /// Sets the Cache-Control header and optionally the Expires and Vary headers.
        /// </summary>
        /// <param name="cacheOptions">A <see cref="CacheOptions"/> object to specify the cache settings.</param>
        public void SetCacheOptions(CacheOptions cacheOptions) {
            if (cacheOptions == null) {
                return;
            }
            if (cacheOptions.Disable) {
                _headers.CacheControl = "no-cache; no-store";
                return;
            }
            _headers.CacheControl = cacheOptions.ToHeaderString();
            if (cacheOptions.AbsoluteExpiry.HasValue) {
                _headers.Expires = cacheOptions.AbsoluteExpiry.Value.ToString("R");
            }

            if (cacheOptions.VaryByHeaders != null && cacheOptions.VaryByHeaders.Count > 0) {
                _headers.Vary = string.Join(", ", cacheOptions.VaryByHeaders);
            }
        }

        public void SetCookie(HttpCookie cookie) {
            _headers.Add(HttpHeaderKeys.SetCookie, cookie.ToResponseHeaderString());
        }

        public void SetLastModified(DateTime when) {
            _headers.LastModified = when.ToUniversalTime()
                                        .ToString("R");
        }

        public void SetLastModified(DateTimeOffset when) {
            _headers.LastModified = when.ToString("r");
        }

        IResponseHeaders IResponse.Headers {
            get { return _headers; }
        }
    }
}