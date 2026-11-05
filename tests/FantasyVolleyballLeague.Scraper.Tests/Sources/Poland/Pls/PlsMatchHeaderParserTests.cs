using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsMatchHeaderParserTests
    {
        [Fact]
        public void ParseFinishedWidgetMatchReadsHeaderBoxNotTicker()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_1104808.html"));

            Assert.Equal(MatchStatus.Finished, header.Status);
            Assert.Equal(new PlsMatchHeaderRoster("GKS Katowice", 18, 3), header.HomeRoster);
            Assert.Equal(new PlsMatchHeaderRoster("Aluron CMC Warta Zawiercie", 2, 0), header.AwayRoster);
            Assert.Equal([new SetScore(26, 24), new SetScore(25, 23), new SetScore(25, 22)], header.Sets);
            Assert.Equal(new DateTime(2026, 10, 16, 15, 30, 0, DateTimeKind.Utc), header.DateUtc);
            Assert.True(header.IsTimeConfirmed);
        }

        [Fact]
        public void ParseScheduledMatchHasNoResultAndUnconfirmedTime()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_1104989.html"));

            Assert.Equal(MatchStatus.Scheduled, header.Status);
            Assert.Empty(header.Sets);
            Assert.Null(header.HomeRoster.SetsWon);
            Assert.Equal(2, header.HomeRoster.ExternalId);
            Assert.False(header.IsTimeConfirmed);
            // 27.03.2027 local midnight (CET, UTC+1).
            Assert.Equal(new DateTime(2027, 3, 26, 23, 0, 0, DateTimeKind.Utc), header.DateUtc);
        }

        [Fact]
        public void ParseArchiveMatchReadsRealSetsWithoutTotals()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_1000675.html"));

            Assert.Equal(MatchStatus.Finished, header.Status);
            Assert.Equal(new PlsMatchHeaderRoster("SKRA BEŁCHATÓW", 61, 3), header.HomeRoster);
            Assert.Equal((3, 0), (header.AwayRoster.ExternalId, header.AwayRoster.SetsWon));
            Assert.Equal([new SetScore(25, 20), new SetScore(25, 23), new SetScore(25, 17)], header.Sets);
            Assert.Equal(("A-1", 600), (header.MatchNumber, header.Attendance));
            Assert.Equal(new DateTime(2005, 3, 18, 18, 0, 0, DateTimeKind.Utc), header.DateUtc);
        }

        [Fact]
        public void ParseMatchReadsMvpWithPlayerId()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_1104115.html"));

            Assert.Equal(new PlayerReference("Benjamin Toniutti", 341), header.Mvp);
            Assert.Equal(("A-1", 2741), (header.MatchNumber, header.Attendance));
            Assert.Equal(3, header.Sets.Count);
        }

        [Fact]
        public void ParseFourSetMatchReadsAllSets()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_5604.html"));

            Assert.Equal(new PlsMatchHeaderRoster("MKS Będzin", 52, 1), header.HomeRoster);
            Assert.Equal((75, 3), (header.AwayRoster.ExternalId, header.AwayRoster.SetsWon));
            Assert.Equal(
                [new SetScore(25, 22), new SetScore(19, 25), new SetScore(20, 25), new SetScore(16, 25)],
                header.Sets);
            Assert.NotNull(header.Mvp?.ExternalId);
        }
    }
}
