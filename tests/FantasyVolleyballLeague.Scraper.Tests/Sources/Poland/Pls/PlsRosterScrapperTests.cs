using FantasyVolleyballLeague.Shared.Constants;
using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsRosterScrapperTests
    {
        private const int Season = 32;
        private const int OtherSeason = 35;
        private const int ClubId = 93;
        private const int OtherClubId = 45;
        private const int StayingPlayerId = 100;
        private const int LeavingPlayerId = 2433;

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

        [Fact]
        public void FormerPlayers_Should_ReturnTheStoredPlayersMissingFromTheTeamPage()
        {
            var teamPagePlayers = new[] { Entry(StayingPlayerId) };
            var stored = Roster(Player(StayingPlayerId), Player(LeavingPlayerId));

            var formerPlayers = PlsRosterScrapper.FormerPlayers(teamPagePlayers, stored);

            formerPlayers.Select(player => player.ExternalId).ShouldBe([LeavingPlayerId]);
        }

        [Fact]
        public void FormerPlayers_Should_ReturnNothing_When_NoFileIsStored()
        {
            var formerPlayers = PlsRosterScrapper.FormerPlayers([Entry(StayingPlayerId)], stored: null);

            formerPlayers.ShouldBeEmpty();
        }

        [Fact]
        public void KeepFormerPlayer_Should_KeepThePlayerWithTheCurrentCareer_When_TheCareerHasAStintInTheClub()
        {
            var career = new[] { Stint(ClubId, new DateOnly(2026, 9, 16), new DateOnly(2026, 11, 25)) };

            var player = PlsRosterScrapper.KeepFormerPlayer(Player(LeavingPlayerId), Details(shirtNumber: 4), career, ClubId);

            player.ShouldNotBeNull().ShouldSatisfyAllConditions(
                () => player.FullName.ShouldBe("Jan Kowalski"),
                () => player.ShirtNumber.ShouldBe(4),
                () => player.Career.ShouldHaveSingleItem().To.ShouldBe(new DateOnly(2026, 11, 25)));
        }

        [Fact]
        public void KeepFormerPlayer_Should_DropThePlayer_When_TheCareerHasNoStintInTheClub()
        {
            var career = new[] { Stint(OtherClubId, new DateOnly(2026, 9, 16), new DateOnly(2027, 3, 30)) };

            var player = PlsRosterScrapper.KeepFormerPlayer(Player(LeavingPlayerId), Details(shirtNumber: 4), career, ClubId);

            player.ShouldBeNull();
        }

        private static PlsRosterEntry Entry(int playerId)
        {
            return new PlsRosterEntry(playerId, "Anna Nowak", 1, null, $"/players/id/{playerId}/tour/53.html");
        }

        private static PlayerProfile Player(int playerId)
        {
            return new PlayerProfile { ExternalId = playerId, FullName = "Jan Kowalski" };
        }

        private static PlsPlayerDetails Details(int shirtNumber)
        {
            return new PlsPlayerDetails(null, null, null, null, null, shirtNumber);
        }

        private static PlayerCareerEntry Stint(int clubId, DateOnly from, DateOnly to)
        {
            return new PlayerCareerEntry { ClubExternalId = clubId, From = from, To = to };
        }

        private static RosterRecord Roster(params PlayerProfile[] players)
        {
            return new RosterRecord
            {
                ScrapedAtUtc = new DateTime(2026, 11, 25, 0, 0, 0, DateTimeKind.Utc),
                League = "PlusLiga",
                Season = new SeasonReference(2026, 2027, 53),
                Club = new ClubLink(ClubId, "Podziemia Kredowe ChKS Chełm"),
                Players = players,
            };
        }

        private static PlsCareerRow Row(int clubId, int tour, DateOnly from, DateOnly to)
        {
            return new PlsCareerRow(clubId, tour, from, to, null, null);
        }
    }
}
