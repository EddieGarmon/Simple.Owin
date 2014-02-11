using System;
using System.Runtime.Serialization;

using Xunit.Sdk;

namespace XunitShould.Sdk
{
    [Serializable]
    internal class EnumerableEqualException : XunitException
    {
        private readonly string _actual;
        private readonly int _actualCount;
        private readonly int _atIndex;
        private readonly string _expected;
        private readonly int _expectedCount;

        public EnumerableEqualException(object expected, object actual, int atIndex, int expectedCount, int actualCount) {
            _expected = expected == null ? "(null)" : expected.ToString();
            _actual = actual == null ? "(null)" : actual.ToString();
            _atIndex = atIndex;
            _expectedCount = expectedCount;
            _actualCount = actualCount;
        }

        /// <inheritdoc/>
        protected EnumerableEqualException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            _expectedCount = info.GetInt32("ExpectedCount");
            _actualCount = info.GetInt32("ActualCount");
            _atIndex = info.GetInt32("AtIndex");
            _expected = info.GetString("Expected");
            _actual = info.GetString("Actual");
        }

        public string Actual {
            get { return _actual; }
        }

        public int ActualCount {
            get { return _actualCount; }
        }

        public int AtIndex {
            get { return _atIndex; }
        }

        public string Expected {
            get { return _expected; }
        }

        public int ExpectedCount {
            get { return _expectedCount; }
        }

        public override string Message {
            get {
                return
                    string.Format(
                        "Enumerables not equal at index: {0}{5}(Expected has {1} items, Actual has {2} items){5}Expected:  {3}{5}Actual: {4}",
                        _atIndex,
                        _expectedCount,
                        _actualCount,
                        _expected,
                        _actual,
                        Environment.NewLine);
            }
        }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            info.AddValue("ExpectedCount", _expectedCount);
            info.AddValue("ActualCount", _actualCount);
            info.AddValue("AtIndex", _atIndex);
            info.AddValue("Expected", _expected);
            info.AddValue("Actual", _actual);

            base.GetObjectData(info, context);
        }
    }
}