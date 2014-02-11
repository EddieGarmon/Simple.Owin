using System;
using System.Runtime.Serialization;

using Xunit.Sdk;

namespace XunitShould.Sdk
{
    internal abstract class RangeException : XunitException
    {
        private readonly string _actual;
        private readonly string _high;
        private readonly bool _highInclusive;
        private readonly string _low;
        private readonly bool _lowInclusive;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RangeException" /> class.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="low">The low.</param>
        /// <param name="lowInclusive">
        ///     if set to <c>true</c> low is considered in the range.
        /// </param>
        /// <param name="high">The high.</param>
        /// <param name="highInclusive">
        ///     if set to <c>true</c> high is considered in the range.
        /// </param>
        /// <param name="messageHeader">exception message header</param>
        /// <remarks></remarks>
        protected RangeException(object actual, object low, bool lowInclusive, object high, bool highInclusive, string messageHeader)
            : base(messageHeader) {
            _actual = actual == null ? null : actual.ToString();
            _low = low == null ? null : low.ToString();
            _lowInclusive = lowInclusive;
            _high = high == null ? null : high.ToString();
            _highInclusive = highInclusive;
        }

        /// <inheritdoc/>
        protected RangeException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            _actual = info.GetString("Actual");
            _low = info.GetString("Low");
            _lowInclusive = info.GetBoolean("LowInclusive");
            _high = info.GetString("High");
            _highInclusive = info.GetBoolean("HighInclusive");
        }

        /// <summary>
        ///     Gets the actual object value
        /// </summary>
        public string Actual {
            get { return _actual; }
        }

        /// <summary>
        ///     Gets the high value of the range
        /// </summary>
        public string High {
            get { return _high; }
        }

        /// <summary>
        ///     Gets a value indicating whether high is considered in the range.
        /// </summary>
        /// <remarks></remarks>
        public bool HighInclusive {
            get { return _highInclusive; }
        }

        /// <summary>
        ///     Gets the low value of the range
        /// </summary>
        public string Low {
            get { return _low; }
        }

        /// <summary>
        ///     Gets a value indicating whether low is considered in the range.
        /// </summary>
        /// <remarks></remarks>
        public bool LowInclusive {
            get { return _lowInclusive; }
        }

        /// <inheritdoc/>
        public override string Message {
            get {
                return string.Format("{0}{6}Range:  {1}{2} - {3}{4}{6}Actual: {5}",
                                     base.Message,
                                     LowInclusive ? "[" : "(",
                                     Low,
                                     High,
                                     HighInclusive ? "]" : ")",
                                     Actual ?? "(null)",
                                     Environment.NewLine);
            }
        }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            info.AddValue("Actual", _actual);
            info.AddValue("Low", _low);
            info.AddValue("LowInclusive", _lowInclusive);
            info.AddValue("High", _high);
            info.AddValue("HighInclusive", _highInclusive);

            base.GetObjectData(info, context);
        }
    }
}