using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Simple.Owin.Extensions;

namespace Simple.Owin
{
    public static class PathMapping
    {
        private static Func<string, string> _map;

        public static Func<string, string> Map {
            get { return _map ?? (_map = BuildDefaultMapper()); }
            set { _map = value; }
        }

        private static Func<string, string> BuildDefaultMapper() {
            return GetSystemWebMapper() ?? GetCodeBaseRelativeMapper();
        }

        private static Func<string, string> GetCodeBaseRelativeMapper() {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var localPath = new Uri(assembly.EscapedCodeBase).LocalPath;
            var path = Path.GetDirectoryName(localPath);
            if (path == null) {
                return null;
            }
            return virtualPath => Path.Combine(path,
                                               virtualPath.TrimStart('/')
                                                          .Replace('/', Path.DirectorySeparatorChar));
        }

        private static Func<string, string> GetSystemWebMapper() {
            var systemWeb = AppDomain.CurrentDomain.GetAssemblies()
                                     .FirstOrDefault(assembly => assembly.FullName.StartsWith("System.Web,"));
            if (systemWeb != null) {
                var hostingEnvironment = systemWeb.GetType("System.Web.Hosting.HostingEnvironment");
                if (hostingEnvironment != null) {
                    return hostingEnvironment.GetDelegate<Func<string, string>>("MapPath", throwOnBindFailure: false);
                }
            }
            return null;
        }
    }
}