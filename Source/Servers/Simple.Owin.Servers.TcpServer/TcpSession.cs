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
using Simple.Owin.Helpers;

namespace Simple.Owin.Servers.Tcp
{
    internal class TcpSession : IDisposable
    {
        private readonly Func<IDictionary<string, object>, Task> _appFunc;
        private readonly TaskCompletionSource<int> _sessionCompleted = new TaskCompletionSource<int>();
        private readonly IDictionary<string, object> _sessionEnvironment;
        private Timer _autoCloseSession;
        private OwinContext _context;
        private string _httpVer;
        private bool _keepAlive;
        private NetworkStream _networkStream;
        private MemoryStream _output;
        private Socket _socket;

        public TcpSession(IDictionary<string, object> owinEnvironment, Func<IDictionary<string, object>, Task> appFunc, Socket socket) {
            _sessionEnvironment = OwinFactory.CreateScopedEnvironment(owinEnvironment);
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
                var requestEnvironment = OwinFactory.CreateScopedEnvironment(_sessionEnvironment);
                _context = OwinContext.Get(requestEnvironment);
                Trace("Session - Process Request");

                //todo: configure OnSendingHeaders aggregator

                // parse request line
                HttpRequestLine requestLine = HttpRequestLine.Parse(_networkStream.ReadLine());
                if (!requestLine.IsValid) {
                    ProcessError(Status.Is.BadRequest);
                    return _sessionCompleted.Task;
                }
                if (!ValidVerbs.Contains(requestLine.Method)) {
                    ProcessError(Status.Is.NotImplemented);
                    return _sessionCompleted.Task;
                }
                _context.Request.PathBase = string.Empty;
                _context.Request.Method = requestLine.Method;
                if (requestLine.Uri.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                    Uri requestUri;
                    if (!Uri.TryCreate(requestLine.Uri, UriKind.Absolute, out requestUri)) {
                        ProcessError(Status.Is.BadRequest);
                        return _sessionCompleted.Task;
                    }
                    _context.Request.FullUri = requestUri;
                }
                else {
                    _context.Request.Scheme = "http";
                    var splitUri = requestLine.Uri.Split('?');
                    _context.Request.Path = splitUri[0];
                    _context.Request.QueryString = splitUri.Length == 2 ? splitUri[1] : string.Empty;
                }
                _context.Request.Protocol = requestLine.HttpVersion;
                _httpVer = requestLine.HttpVersion.Substring(requestLine.HttpVersion.IndexOf('/') + 1);

                // parse http headers
                var headers = new List<string>();
                while (true) {
                    string headerLine = _networkStream.ReadLine();
                    if (headerLine == string.Empty) {
                        break;
                    }
                    headers.Add(headerLine);
                }
                _context.Request.Headers.AddRaw(headers);

                _keepAlive = (_httpVer == "1.0" && _context.Request.Headers.ValueIs(HttpHeaderKeys.Connection, "Keep-Alive", false)) ||
                             !_context.Request.Headers.ValueIs(HttpHeaderKeys.Connection, "Close", false);
                _context.Request.Body = _networkStream;
                _context.Response.Body = _output;

                // handle 100-continue
                _context.Request.Headers.GetValue(HttpHeaderKeys.Expect);
                if (_context.Request.Headers.ValueIs(HttpHeaderKeys.Expect, "100-Continue", false)) {
                    _networkStream.WriteAsync("HTTP/1.1 100 Continue\r\n", Encoding.UTF8)
                                  .ContinueWith(task => {
                                                    if (task.IsFaulted) {
                                                        SessionFaulted(task.Exception);
                                                        return;
                                                    }
                                                    _appFunc(_context.Environment)
                                                        .ContinueWith(ProcessResult);
                                                });
                }
                else {
                    _appFunc(_context.Environment)
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
                ProcessError(Status.Is.InternalServerError);
                return;
            }

            var status = _context.Response.Status;
            if (status.IsError) {
                ProcessError(status);
                return;
            }

            //todo: execute OnSendingHeaders

            var headerBuilder = new StringBuilder();
            headerBuilder.Append(status.ToHttp11StatusLine());

            var headers = _context.Response.Headers;
            if (headers.ContentLength < 0) {
                headers.ContentLength = _output.Length;
            }
            if (_httpVer == "1.0" && _keepAlive) {
                headers.Connection = "Keep-Alive";
            }
            if (!_keepAlive) {
                headers.Connection = "Close";
            }
            const string headerLineFormat = "{0}: {1}\r\n";
            foreach (var header in headers.Raw) {
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
            Dispose();
        }

        private void SessionFaulted(Exception exception = null) {
            _sessionCompleted.SetException(exception ?? new Exception("Unknown error."));
            Dispose();
        }

        private void Trace(string message) {
            var output = _context.TraceOutput;
            if (output != null) {
                output.WriteLine(message);
            }
        }

        private static readonly string[] ValidVerbs = { "OPTIONS", "GET", "HEAD", "POST", "PUT", "DELETE", "TRACE", "CONNECT" };
    }
}