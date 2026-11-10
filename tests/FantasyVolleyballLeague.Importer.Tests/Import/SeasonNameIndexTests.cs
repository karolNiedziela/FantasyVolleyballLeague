using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class SeasonNameIndexTests
    {
        private const int Men = 1;
        private const int Women = 2;

        [Fact]
        public void FindReturnsThePlayerOfThatNameInTheSeason()
        {
            var index = new SeasonNameIndex();
            var player = Guid.CreateVersion7();
            index.Add(Men, 2019, PlayerNameKey.Create("Michał Filip"), player);

            index.Find(Men, 2019, PlayerNameKey.Create("Filip Michal")).ShouldBe(player);
        }

        [Fact]
        public void FindCountsOnePlayerOnTwoRostersOnce()
        {
            var index = new SeasonNameIndex();
            var player = Guid.CreateVersion7();
            index.Add(Men, 2019, "FILIP MICHAL", player);
            index.Add(Men, 2019, "FILIP MICHAL", player);

            index.Find(Men, 2019, "FILIP MICHAL").ShouldBe(player);
        }

        [Fact]
        public void FindReturnsNothingWhenTwoPlayersShareTheName()
        {
            var index = new SeasonNameIndex();
            index.Add(Men, 2019, "NOWAK JAN", Guid.CreateVersion7());
            index.Add(Men, 2019, "NOWAK JAN", Guid.CreateVersion7());

            index.Find(Men, 2019, "NOWAK JAN").ShouldBeNull();
        }

        [Fact]
        public void FindIgnoresOtherGendersAndSeasons()
        {
            var index = new SeasonNameIndex();
            index.Add(Women, 2019, "WOJCIK MARTA", Guid.CreateVersion7());

            index.Find(Men, 2019, "WOJCIK MARTA").ShouldBeNull();
            index.Find(Women, 2020, "WOJCIK MARTA").ShouldBeNull();
        }
    }
}
