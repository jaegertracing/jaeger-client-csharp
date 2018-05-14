using Jaeger.Core.Baggage;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class DefaultBaggageRestrictionManagerTest
    {
        [Fact]
        public void TestGetRestriction()
        {
            DefaultBaggageRestrictionManager undertest = new DefaultBaggageRestrictionManager();

            string key = "key";
            Restriction actual = undertest.GetRestriction("", key);
            Restriction expected = new Restriction(true, 2048);
            Assert.Equal(expected, actual);

            expected = actual;
            actual = undertest.GetRestriction("", key);
            Assert.Same(actual, expected);
        }
    }
}
