namespace XunitShould.Sdk
{
    internal class NotInRangeException : RangeException
    {
        public NotInRangeException(object actual, object low, bool lowInclusive, object high, bool highInclusive)
            : base(actual, low, lowInclusive, high, highInclusive, "Assert.NotInRange() Failure") { }
    }
}