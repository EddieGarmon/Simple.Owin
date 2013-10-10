using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Simple.Owin.Extensions;

namespace Simple.Owin
{
    public class OwinContext : IContext
    {
        private readonly IDictionary<string, object> _environment;
        private readonly OwinRequest _request;
        private readonly OwinResponse _response;

        private OwinContext(IDictionary<string, object> environment) {
            if (environment == null) {
                throw new ArgumentNullException("environment");
            }
            _environment = environment;
            if (!_environment.ContainsKey(OwinKeys.Owin.CallCancelled)) {
                _environment.Add(OwinKeys.Owin.CallCancelled, new CancellationToken());
            }
            _request = new OwinRequest(environment);
            _response = new OwinResponse(environment);
        }

        public CancellationToken CancellationToken {
            get { return _environment.GetValue<CancellationToken>(OwinKeys.Owin.CallCancelled); }
        }

        public IDictionary<string, object> Environment {
            get { return _environment; }
        }

        public string OwinVersion {
            get { return _environment.GetValue<string>(OwinKeys.Owin.Version); }
            set { _environment.SetValue(OwinKeys.Owin.Version, value); }
        }

        public OwinRequest Request {
            get { return _request; }
        }

        public OwinResponse Response {
            get { return _response; }
        }

        public string DumpEnvironmentAsHtmlTable() {
            var builder = new StringBuilder();
            builder.Append("<table><tr><th>Key</th><th>Value</th></tr>");
            List<string> keys = Environment.Keys.OrderBy(key => key)
                                           .ToList();
            foreach (var key in keys) {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", key, Environment[key]);
            }
            builder.Append("</table>");
            return builder.ToString();
        }

        IRequest IContext.Request {
            get { return _request; }
        }

        IResponse IContext.Response {
            get { return _response; }
        }

        public static OwinContext Get(IDictionary<string, object> environment) {
            return environment.GetValueOrCreate(OwinKeys.Simple.Context, () => new OwinContext(environment));
        }
    }
}