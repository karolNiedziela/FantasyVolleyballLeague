using FantasyVolleyballLeague.Shared.Constants;
using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsRosterScrapperTests
    {
        private const int Season = 32;
        private const int OtherSeason = 35;

        [Fact]
        public void ToCareerEntries_Should_KeepOnlyTheStintsOfTheGivenSeason()
        {
            var rows = new[]
            {
                Row(clubId: 83, tour: Season, from: new DateOnly(2019, 10, 25), to: new DateOnly(2020, 1, 30)),
                Row(clubId: 68, tour: OtherSeason, from: new DateOnly(2020, 9, 11), to: new DateOnly(2021, 5, 9)),
                Row(clubId: 15, tour: Season, from: new DateOnly(2020, 1, 30), to: new DateOnly(2020, 3, 25)),
            };

            var entries = PlsRosterScrapper.ToCareerEntries(rows, Season);

            entries.Select(entry => entry.ClubExternalId).ShouldBe([83, 15]);
        }

        [Fact]
        public void ToCareerEntries_Should_OrderTheStintsByStartDate()
        {
            var rows = new[]
            {
                Row(clubId: 15, tour: Season, from: new DateOnly(2020, 1, 30), to: new DateOnly(2020, 3, 25)),
                Row(clubId: 83, tour: Season, from: new DateOnly(2019, 10, 25), to: new DateOnly(2020, 1, 30)),
            };

            var entries = PlsRosterScrapper.ToCareerEntries(rows, Season);

            entries.Select(entry => entry.ClubExternalId).ShouldBe([83, 15]);
        }

        [Fact]
        public void ToCareerEntries_Should_CopyTheFieldsOfTheStint()
        {
            var rows = new[]
            {
                new PlsCareerRow(83, Season, new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30), 10, PositionNames.OppositeHitter),
            };

            var entries = PlsRosterScrapper.ToCareerEntries(rows, Season);

            entries.ShouldHaveSingleItem().ShouldBe(new PlayerCareerEntry
            {
                ClubExternalId = 83,
                From = new DateOnly(2019, 10, 25),
                To = new DateOnly(2020, 1, 30),
                ShirtNumber = 10,
                Position = PositionNames.OppositeHitter,
            });
        }

        [Fact]
        public void ToCareerEntries_Should_ReturnNothing_When_TheSeasonIsUnknown()
        {
            var rows = new[] { Row(clubId: 83, tour: Season, from: new DateOnly(2019, 10, 25), to: new DateOnly(2020, 1, 30)) };

            var entries = PlsRosterScrapper.ToCareerEntries(rows, null);

            entries.ShouldBeEmpty();
        }

        private static PlsCareerRow Row(int clubId, int tour, DateOnly from, DateOnly to)
        {
            return new PlsCareerRow(clubId, tour, from, to, null, null);
        }
    }
}
