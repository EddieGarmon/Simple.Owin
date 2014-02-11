namespace XunitShould.Sdk
{
    internal class InRangeException : RangeException
    {
        public InRangeException(object actual, object low, bool lowInclusive, object high, bool highInclusive)
            : base(actual, low, lowInclusive, high, highInclusive, "Assert.InRange() Failure") { }
    }
}