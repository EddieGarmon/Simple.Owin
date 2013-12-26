using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    internal class XunitException : AssertException
    {
        public XunitException() { }

        public XunitException(string message)
            : base(message) { }

        protected XunitException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}