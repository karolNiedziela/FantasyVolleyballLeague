using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class RosterMatchingTests
    {
        private static readonly Guid Filip = new("00000000-0000-0000-0000-000000000005");
        private static readonly Guid Jan = new("00000000-0000-0000-0000-000000000001");
        private static readonly Guid Piotr = new("00000000-0000-0000-0000-000000000002");
        private static readonly Guid Kowalski = new("00000000-0000-0000-0000-000000000003");
        private static readonly Guid Namesake = new("00000000-0000-0000-0000-000000000004");

        [Fact]
        public void FindMatchesShirtNumberConfirmedByName()
        {
            var roster = new[] { Member(Filip, 10, "Michal Filip", null, null) };

            var found = RosterMatching.Find(roster, 10, PlayerNameKey.Create("Filip Michał"), new DateOnly(2019, 11, 1));

            found.ShouldBe(Filip);
        }

        [Fact]
        public void FindMatchesNameWhenTheNumberDiffers()
        {
            var roster = new[] { Member(Filip, 94, "Michal Filip", null, null) };

            var found = RosterMatching.Find(roster, 10, PlayerNameKey.Create("Filip Michał"), null);

            found.ShouldBe(Filip);
        }

        [Fact]
        public void Find_Should_ReturnTheEarlierPlayer_When_TheMatchDateIsInHisStintAndTheShirtNumberWasReused()
        {
            var roster = TwoNowaksWearingNumberTen();

            var found = RosterMatching.Find(roster, 10, PlayerNameKey.Create("Nowak"), new DateOnly(2019, 12, 1));

            found.ShouldBe(Jan);
        }

        [Fact]
        public void Find_Should_ReturnTheLaterPlayer_When_TheMatchDateIsInHisStintAndTheShirtNumberWasReused()
        {
            var roster = TwoNowaksWearingNumberTen();

            var found = RosterMatching.Find(roster, 10, PlayerNameKey.Create("Nowak"), new DateOnly(2020, 3, 1));

            found.ShouldBe(Piotr);
        }

        [Fact]
        public void Find_Should_ReturnNull_When_TheShirtNumberBelongsToAPlayerWithADifferentName()
        {
            var roster = new[] { Member(Filip, 10, "Michal Filip", null, null) };

            var found = RosterMatching.Find(roster, 10, PlayerNameKey.Create("Jan Kowalski"), null);

            found.ShouldBeNull();
        }

        [Fact]
        public void Find_Should_MatchByShirtNumber_When_TheNamesShareOnlyTheSurname()
        {
            var roster = new[] { Member(Kowalski, 10, "Jan Kowalski", null, null) };

            var found = RosterMatching.Find(roster, 10, PlayerNameKey.Create("Kowalski J."), null);

            found.ShouldBe(Kowalski);
        }

        [Fact]
        public void Find_Should_FallBackToTheName_When_TwoPlayersWearTheSameShirtNumber()
        {
            var roster = new[]
            {
                Member(Filip, 10, "Michal Filip", null, null),
                Member(Piotr, 10, "Piotr Nowak", null, null),
            };

            var found = RosterMatching.Find(roster, 10, PlayerNameKey.Create("Nowak Piotr"), null);

            found.ShouldBe(Piotr);
        }

        [Fact]
        public void Find_Should_ReturnNull_When_TheNameMatchesTwoPlayers()
        {
            var roster = new[]
            {
                Member(Filip, 10, "Michal Filip", null, null),
                Member(Namesake, 11, "Filip Michal", null, null),
            };

            var found = RosterMatching.Find(roster, null, PlayerNameKey.Create("Michal Filip"), null);

            found.ShouldBeNull();
        }

        [Fact]
        public void Find_Should_MatchByName_When_TheShirtNumberIsUnknown()
        {
            var roster = new[] { Member(Filip, 10, "Michal Filip", null, null) };

            var found = RosterMatching.Find(roster, null, PlayerNameKey.Create("Filip Michał"), null);

            found.ShouldBe(Filip);
        }

        [Fact]
        public void FindFallsBackToTheWholeRosterWhenNobodyCoversTheDate()
        {
            var roster = new[] { Member(Filip, 10, "Michal Filip", new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30)) };

            var found = RosterMatching.Find(roster, 10, PlayerNameKey.Create("Filip Michał"), new DateOnly(2020, 6, 1));

            found.ShouldBe(Filip);
        }

        [Fact]
        public void FindReturnsNothingForAnUnknownPlayer()
        {
            var roster = new[] { Member(Filip, 10, "Michal Filip", null, null) };

            RosterMatching.Find(roster, 7, PlayerNameKey.Create("Jan Kowalski"), new DateOnly(2019, 11, 1)).ShouldBeNull();
        }

        private static RosterMember[] TwoNowaksWearingNumberTen()
        {
            return
            [
                Member(Jan, 10, "Jan Nowak", new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30)),
                Member(Piotr, 10, "Piotr Nowak", new DateOnly(2020, 2, 5), new DateOnly(2020, 5, 1)),
            ];
        }

        private static RosterMember Member(Guid id, int number, string name, DateOnly? from, DateOnly? to)
        {
            return new RosterMember(id, number, PlayerNameKey.Create(name), from, to);
        }
    }
}
