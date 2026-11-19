using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class SeasonNameIndexTests
    {
        private const int Men = 1;
        private const int Women = 2;

        private static readonly Guid Filip = new("00000000-0000-0000-0000-000000000001");
        private static readonly Guid Nowak = new("00000000-0000-0000-0000-000000000002");
        private static readonly Guid NowakNamesake = new("00000000-0000-0000-0000-000000000003");
        private static readonly Guid Wojcik = new("00000000-0000-0000-0000-000000000004");

        [Fact]
        public void FindReturnsThePlayerOfThatNameInTheSeason()
        {
            var index = new SeasonNameIndex();
            index.Add(Men, 2019, PlayerNameKey.Create("Michał Filip"), Filip);

            index.Find(Men, 2019, PlayerNameKey.Create("Filip Michal")).ShouldBe(Filip);
        }

        [Fact]
        public void FindCountsOnePlayerOnTwoRostersOnce()
        {
            var index = new SeasonNameIndex();
            index.Add(Men, 2019, "FILIP MICHAL", Filip);
            index.Add(Men, 2019, "FILIP MICHAL", Filip);

            index.Find(Men, 2019, "FILIP MICHAL").ShouldBe(Filip);
        }

        [Fact]
        public void FindReturnsNothingWhenTwoPlayersShareTheName()
        {
            var index = new SeasonNameIndex();
            index.Add(Men, 2019, "NOWAK JAN", Nowak);
            index.Add(Men, 2019, "NOWAK JAN", NowakNamesake);

            index.Find(Men, 2019, "NOWAK JAN").ShouldBeNull();
        }

        [Fact]
        public void FindIgnoresOtherGendersAndSeasons()
        {
            var index = new SeasonNameIndex();
            index.Add(Women, 2019, "WOJCIK MARTA", Wojcik);

            index.Find(Men, 2019, "WOJCIK MARTA").ShouldBeNull();
            index.Find(Women, 2020, "WOJCIK MARTA").ShouldBeNull();
        }
    }
}
