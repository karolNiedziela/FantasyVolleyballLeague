using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class StatisticsParsingTests
    {
        [Theory]
        [InlineData("41%", 41)]
        [InlineData("41.5%", 41.5)]
        [InlineData("41,5%", 41.5)]
        [InlineData("0", 0)]
        [InlineData("-12%", -12)]
        public void ParsePercentReadsWebsiteFormats(string value, double expected)
        {
            StatisticsParsing.ParsePercent(value).ShouldBe((decimal)expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("-")]
        public void ParsePercentReturnsNullWithoutValue(string? value)
        {
            StatisticsParsing.ParsePercent(value).ShouldBeNull();
        }

        [Fact]
        public void ToUtcOffsetHasZeroOffset()
        {
            var offset = StatisticsParsing.ToUtcOffset(new DateTime(2026, 10, 16, 15, 30, 0, DateTimeKind.Unspecified));

            offset.Offset.ShouldBe(TimeSpan.Zero);
            offset.UtcDateTime.ShouldBe(new DateTime(2026, 10, 16, 15, 30, 0, DateTimeKind.Utc));
        }
    }
}
