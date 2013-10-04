using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Simple.Owin.Extensions;
using Simple.Owin.Http;

namespace Simple.Owin.Support
{
    internal class OwinResponse : IResponse
    {
        private readonly IDictionary<string, object> _environment;

        public OwinResponse(IDictionary<string, object> environment) {
            _environment = environment;
        }

        public IDictionary<string, string[]> Headers {
            get { return _environment.GetValue<IDictionary<string, string[]>>(OwinKeys.Response.Headers); }
        }

        public Stream Output {
            get { return _environment.GetValue<Stream>(OwinKeys.Response.Body); }
        }

        public Status Status {
            get { throw new NotImplementedException(); }
            set {
                //todo handle status of '0'
                _environment.SetValue(OwinKeys.Response.StatusCode, value.Code);
                _environment.SetValue(OwinKeys.Response.ReasonPhrase, value.Description);
                if (value.LocationHeader != null) {
                    Headers[HttpHeaderKeys.Location] = new[] { value.LocationHeader };
                }
            }
        }

        public Func<Stream, Task> WriteFunction { get; set; }
    }
}