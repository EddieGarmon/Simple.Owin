using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Simple.Owin.Extensions;
using Simple.Owin.Http;
using Simple.Owin.Support;

namespace Simple.Owin.Servers.TcpServer
{
    internal class Session : IDisposable
    {
        private static readonly string[] ValidVerbs = { "OPTIONS", "GET", "HEAD", "POST", "PUT", "DELETE", "TRACE", "CONNECT" };

        private readonly Func<IDictionary<string, object>, Task> _appFunc;
        private readonly TaskCompletionSource<int> _sessionCompleted = new TaskCompletionSource<int>();
        private readonly IDictionary<string, object> _sessionEnvironment;
        private Timer _autoCloseSession;
        private string _httpVer;
        private bool _keepAlive;
        private NetworkStream _networkStream;
        private MemoryStream _output;
        private IDictionary<string, object> _requestEnvironment;
        private Socket _socket;

        public Session(IDictionary<string, object> owinEnvironment, Func<IDictionary<string, object>, Task> appFunc, Socket socket) {
            _sessionEnvironment = new Dictionary<string, object>(owinEnvironment, StringComparer.OrdinalIgnoreCase);
            _appFunc = appFunc;
            _socket = socket;

            var remote = socket.RemoteEndPoint as IPEndPoint;
            if (remote != null) {
                _sessionEnvironment.Add(OwinKeys.Server.RemoteIpAddress, remote.Address.ToString());
                _sessionEnvironment.Add(OwinKeys.Server.RemotePort, remote.Port.ToString(CultureInfo.InvariantCulture));
            }

            _networkStream = new NetworkStream(_socket, FileAccess.ReadWrite, true);
            _output = new MemoryStream();
        }

        public void Dispose() {
            if (_networkStream != null) {
                _networkStream.Dispose();
                _networkStream = null;
            }
            if (_socket != null) {
                _socket.Dispose();
                _socket = null;
            }
            if (_output != null) {
                _output.Dispose();
                _output = null;
            }
            if (_autoCloseSession != null) {
                _autoCloseSession.Dispose();
                _autoCloseSession = null;
            }
        }

        public Task ProcessRequest() {
            try {
                //build out request environment
                _requestEnvironment = new Dictionary<string, object>(_sessionEnvironment, StringComparer.Ordinal);
                var context = new OwinContextBuilder(_requestEnvironment);
                // parse request line
                string headerLine = _networkStream.ReadHttpHeaderLine();
                // todo gracefully handle empty first line 
                if (headerLine == null) {
                    ProcessError(Status.Error.BadRequest);
                    return _sessionCompleted.Task;
                }
                string[] requestParts = headerLine.Split(' ');
                if (requestParts.Length != 3) {
                    ProcessError(Status.Error.BadRequest);
                    return _sessionCompleted.Task;
                }
                if (!ValidVerbs.Contains(requestParts[0])) {
                    ProcessError(Status.Error.NotImplemented);
                    return _sessionCompleted.Task;
                }
                context.SetHttpMethod(requestParts[0]);
                context.SetPathBase(string.Empty);
                if (requestParts[1].StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                    Uri requestUri;
                    if (!Uri.TryCreate(requestParts[1], UriKind.Absolute, out requestUri)) {
                        ProcessError(Status.Error.BadRequest);
                        return _sessionCompleted.Task;
                    }
                    context.SetFullUri(requestUri);
                }
                else {
                    context.SetScheme("http");
                    var splitUri = requestParts[1].Split('?');
                    context.SetPath(splitUri[0]);
                    context.SetQueryString(splitUri.Length == 2 ? splitUri[1] : string.Empty);
                }
                context.SetProtocol(requestParts[2]);
                _httpVer = requestParts[2].Substring(requestParts[2].IndexOf('/') + 1);
                // parse http headers
                while (true) {
                    headerLine = _networkStream.ReadHttpHeaderLine();
                    if (headerLine == null) {
                        ProcessError(Status.Error.BadRequest);
                        return _sessionCompleted.Task;
                    }
                    if (headerLine == string.Empty) {
                        break;
                    }
                    int colon = headerLine.IndexOf(':');
                    context.AddRequestHeader(headerLine.Substring(0, colon), headerLine.Substring(colon + 1));
                    // todo: handle multi line
                }

                _keepAlive = (_httpVer == "1.0" && context.HasRequestHeader("Connection", "Keep-Alive", true)) ||
                             !context.HasRequestHeader("Connection", "Close", true);
                context.SetRequestStream(_networkStream);
                context.PrepareResponseHeaders();
                context.SetResponseStream(_output);

                // handle 100-continue
                if (context.HasRequestHeader(HttpHeaderKeys.Expect, "100-Continue", true)) {
                    _networkStream.WriteAsync("HTTP/1.1 100 Continue\r\n", Encoding.UTF8)
                                  .ContinueWith(task => {
                                                    if (task.IsFaulted) {
                                                        SessionFaulted(task.Exception);
                                                        return;
                                                    }
                                                    _appFunc(_requestEnvironment)
                                                        .ContinueWith(ProcessResult);
                                                });
                }
                else {
                    _appFunc(_requestEnvironment)
                        .ContinueWith(ProcessResult);
                }
            }
            catch (Exception exception) {
                SessionFaulted(exception);
            }
            return _sessionCompleted.Task;
        }

        private void ProcessError(Status status) {
            var headerBuilder = new StringBuilder();
            headerBuilder.AppendFormat("HTTP/1.1 {0} {1}\r\n", status.Code, status.Description);
            if (!_keepAlive) {
                headerBuilder.Append("Connection: close\r\n");
            }
            headerBuilder.Append("\r\n");
            _networkStream.WriteAsync(Encoding.UTF8.GetBytes(headerBuilder.ToString()))
                          .ContinueWith(ProcessKeepAlive);
        }

        private void ProcessKeepAlive(Task previous) {
            if (previous.IsFaulted) {
                SessionFaulted(previous.Exception);
                return;
            }
            if (previous.IsCanceled) {
                SessionCanceled();
                return;
            }
            if (!_keepAlive) {
                Dispose();
                return;
            }
            if (_socket == null) {
                return;
            }
            _autoCloseSession = new Timer(_ => {
                                              _sessionCompleted.SetResult(0);
                                              Dispose();
                                          },
                                          null,
                                          2000,
                                          2000);

            _socket.BeginReceive(new byte[1],
                                 0,
                                 1,
                                 SocketFlags.Peek,
                                 ar => {
                                     try {
                                         if (_socket == null) {
                                             return;
                                         }
                                         if (_autoCloseSession != null) {
                                             _autoCloseSession.Dispose();
                                             _autoCloseSession = null;
                                         }
                                         int size = _socket.EndReceive(ar);
                                         if (size > 0) {
                                             _output.Reset();
                                             ProcessRequest();
                                         }
                                         else {
                                             Dispose();
                                             _sessionCompleted.SetResult(0);
                                         }
                                     }
                                     catch (ObjectDisposedException) {
                                         Dispose();
                                     }
                                 },
                                 null);
        }

        private void ProcessResult(Task previous) {
            if (previous.IsFaulted) {
                _keepAlive = false;
                ProcessError(Status.Error.InternalServerError);
                return;
            }

            int statusCode = _requestEnvironment.GetValueOrDefault(OwinKeys.Response.StatusCode, 200);
            if (statusCode == 404) {
                ProcessError(Status.Error.NotFound);
                return;
            }

            var headerBuilder = new StringBuilder();
            headerBuilder.AppendFormat("HTTP/1.1 {0} {1}\r\n",
                                       _requestEnvironment.GetValueOrDefault(OwinKeys.Response.StatusCode, 200),
                                       _requestEnvironment.GetValueOrDefault(OwinKeys.Response.ReasonPhrase, string.Empty));

            var headers = _requestEnvironment.GetValueOrDefault<IDictionary<string, string[]>>(OwinKeys.Response.Headers);
            if (!headers.ContainsKey(HttpHeaderKeys.ContentLength)) {
                headers[HttpHeaderKeys.ContentLength] = new[] { _output.Length.ToString(CultureInfo.InvariantCulture) };
            }
            if (_httpVer == "1.0" && _keepAlive) {
                headers[HttpHeaderKeys.Connection] = new[] { "Keep-Alive" };
            }
            //todo: support headers[Connection] = close
            const string headerLineFormat = "{0}: {1}\r\n";
            foreach (var header in headers) {
                switch (header.Value.Length) {
                    case 0:
                        break;
                    case 1:
                        headerBuilder.AppendFormat(headerLineFormat, header.Key, header.Value[0]);
                        break;
                    default:
                        foreach (var value in header.Value) {
                            headerBuilder.AppendFormat(headerLineFormat, header.Key, value);
                        }
                        break;
                }
            }

            headerBuilder.Append("\r\n");
            var task = _networkStream.WriteAsync(headerBuilder.ToString(), Encoding.UTF8);
            if (task.IsFaulted) {
                SessionFaulted(task.Exception);
                return;
            }
            if (task.IsCanceled) {
                SessionCanceled();
                return;
            }
            if (_output.Length > 0) {
                task = task.ContinueWith(t => {
                                             _output.SeekToBegin();
                                             _output.CopyTo(_networkStream);
                                             return TaskHelper.Completed();
                                         })
                           .Unwrap();
            }

            task.ContinueWith(ProcessKeepAlive);
        }

        private void SessionCanceled() {
            _sessionCompleted.SetCanceled();
            //Dispose();
        }

        private void SessionFaulted(Exception exception = null) {
            _sessionCompleted.SetException(exception ?? new Exception("Unknown error."));
            Dispose();
        }
    }
}