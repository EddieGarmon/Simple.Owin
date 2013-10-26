using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Simple.Owin.Extensions;
using Simple.Owin.Helpers;

namespace Simple.Owin
{
    public class FormData
    {
        private readonly Dictionary<string, string> _data = new Dictionary<string, string>();

        public int Count {
            get { return _data.Count; }
        }

        public string this[string name] {
            get {
                string value;
                return _data.TryGetValue(name, out value) ? value : null;
            }
            private set { _data[name] = value; }
        }

        public const string FormUrlEncoded = "application/x-www-form-urlencoded";

        private static readonly char[] SplitTokens = { '\n', '&' };

        /// <summary>
        /// Builds a FormData from the specified input stream, 
        /// which is assumed to be in x-www-form-urlencoded format.
        /// </summary>
        /// <param name="stream">the input stream</param>
        /// <returns>a populated FormData object</returns>
        public static Task<FormData> Parse(Stream stream) {
            var form = new FormData();
            string input = stream.ReadAll();
            var pairs = input.Split(SplitTokens, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs) {
                var nameValue = pair.Split('=');
                form[nameValue[0]] = UrlHelper.Decode(nameValue[1]);
            }
            return TaskHelper.Completed(form);
        }
    }
}