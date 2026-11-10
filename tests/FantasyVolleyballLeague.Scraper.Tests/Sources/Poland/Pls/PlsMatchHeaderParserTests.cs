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

            header.Status.ShouldBe(MatchStatus.Finished);
            header.HomeRoster.ShouldBe(new PlsMatchHeaderRoster("GKS Katowice", 18, 3));
            header.AwayRoster.ShouldBe(new PlsMatchHeaderRoster("Aluron CMC Warta Zawiercie", 2, 0));
            header.Sets.ShouldBe([new SetScore(26, 24), new SetScore(25, 23), new SetScore(25, 22)]);
            header.DateUtc.ShouldBe(new DateTime(2026, 10, 16, 15, 30, 0, DateTimeKind.Utc));
            header.IsTimeConfirmed.ShouldBeTrue();
        }

        [Fact]
        public void ParseScheduledMatchHasNoResultAndUnconfirmedTime()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_1104989.html"));

            header.Status.ShouldBe(MatchStatus.Scheduled);
            header.Sets.ShouldBeEmpty();
            header.HomeRoster.SetsWon.ShouldBeNull();
            header.HomeRoster.ExternalId.ShouldBe(2);
            header.IsTimeConfirmed.ShouldBeFalse();
            // 27.03.2027 local midnight (CET, UTC+1).
            header.DateUtc.ShouldBe(new DateTime(2027, 3, 26, 23, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void ParseArchiveMatchReadsRealSetsWithoutTotals()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_1000675.html"));

            header.Status.ShouldBe(MatchStatus.Finished);
            header.HomeRoster.ShouldBe(new PlsMatchHeaderRoster("SKRA BEŁCHATÓW", 61, 3));
            (header.AwayRoster.ExternalId, header.AwayRoster.SetsWon).ShouldBe((3, 0));
            header.Sets.ShouldBe([new SetScore(25, 20), new SetScore(25, 23), new SetScore(25, 17)]);
            (header.MatchNumber, header.Attendance).ShouldBe(("A-1", 600));
            header.DateUtc.ShouldBe(new DateTime(2005, 3, 18, 18, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void ParseMatchReadsMvpWithPlayerId()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_1104115.html"));

            header.Mvp.ShouldBe(new PlayerReference("Benjamin Toniutti", 341));
            (header.MatchNumber, header.Attendance).ShouldBe(("A-1", 2741));
            header.Sets.Count.ShouldBe(3);
        }

        [Fact]
        public void ParseFourSetMatchReadsAllSets()
        {
            var header = PlsMatchHeaderParser.Parse(Fixture.Load("match_5604.html"));

            header.HomeRoster.ShouldBe(new PlsMatchHeaderRoster("MKS Będzin", 52, 1));
            (header.AwayRoster.ExternalId, header.AwayRoster.SetsWon).ShouldBe((75, 3));
            header.Sets.ShouldBe([new SetScore(25, 22), new SetScore(19, 25), new SetScore(20, 25), new SetScore(16, 25)]);
            header.Mvp?.ExternalId.ShouldNotBeNull();
        }
    }
}
