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

            Assert.Equal(player, index.Find(Men, 2019, PlayerNameKey.Create("Filip Michal")));
        }

        [Fact]
        public void FindCountsOnePlayerOnTwoRostersOnce()
        {
            var index = new SeasonNameIndex();
            var player = Guid.CreateVersion7();
            index.Add(Men, 2019, "FILIP MICHAL", player);
            index.Add(Men, 2019, "FILIP MICHAL", player);

            Assert.Equal(player, index.Find(Men, 2019, "FILIP MICHAL"));
        }

        [Fact]
        public void FindReturnsNothingWhenTwoPlayersShareTheName()
        {
            var index = new SeasonNameIndex();
            index.Add(Men, 2019, "NOWAK JAN", Guid.CreateVersion7());
            index.Add(Men, 2019, "NOWAK JAN", Guid.CreateVersion7());

            Assert.Null(index.Find(Men, 2019, "NOWAK JAN"));
        }

        [Fact]
        public void FindIgnoresOtherGendersAndSeasons()
        {
            var index = new SeasonNameIndex();
            index.Add(Women, 2019, "WOJCIK MARTA", Guid.CreateVersion7());

            Assert.Null(index.Find(Men, 2019, "WOJCIK MARTA"));
            Assert.Null(index.Find(Women, 2020, "WOJCIK MARTA"));
        }
    }
}
