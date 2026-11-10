using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class RosterMatchingTests
    {
        private static readonly Guid Filip = Guid.CreateVersion7();
        private static readonly Guid Newcomer = Guid.CreateVersion7();

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
        public void FindUsesTheMatchDateWhenAShirtNumberWasWornByTwoPlayers()
        {
            var roster = new[]
            {
                Member(Filip, 10, "Michal Filip", new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30)),
                Member(Newcomer, 10, "Jan Nowak", new DateOnly(2020, 2, 5), new DateOnly(2020, 5, 1)),
            };

            RosterMatching.Find(roster, 10, PlayerNameKey.Create("Filip Michał"), new DateOnly(2019, 12, 1)).ShouldBe(Filip);
            RosterMatching.Find(roster, 10, PlayerNameKey.Create("Nowak Jan"), new DateOnly(2020, 3, 1)).ShouldBe(Newcomer);
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

        private static RosterMember Member(Guid id, int number, string name, DateOnly? from, DateOnly? to)
        {
            return new RosterMember(id, number, PlayerNameKey.Create(name), from, to);
        }
    }
}
