using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class LeagueNamingTests
    {
        private static readonly string[] LeagueKeys = ["PlusLiga", "PLS1Liga"];

        private static readonly LeagueNameFile[] Entries =
        [
            new("PlusLiga", 2026, "TAURON Liga"),
            new("PlusLiga", 1928, "Polska Liga Siatkówki"),
            new("PlusLiga", 2007, "PlusLiga"),
            new("PLS1Liga", 2018, "PLS 1 Liga"),
        ];

        [Theory]
        [InlineData(2000, "Polska Liga Siatkówki")]
        [InlineData(2006, "Polska Liga Siatkówki")]
        [InlineData(2007, "PlusLiga")]
        [InlineData(2025, "PlusLiga")]
        [InlineData(2026, "TAURON Liga")]
        [InlineData(2030, "TAURON Liga")]
        public void Find_Should_ReturnTheLatestEntryNotAfterTheSeason(int startYear, string expected)
        {
            LeagueNaming.Find(Entries, "PlusLiga", startYear).ShouldBe(expected);
        }

        [Fact]
        public void Find_Should_ReturnNull_When_TheLeagueHasNoEntryUntilTheSeason()
        {
            LeagueNaming.Find(Entries, "PLS1Liga", 2017).ShouldBeNull();
        }

        [Fact]
        public void Find_Should_ReturnNull_When_TheLeagueHasNoEntries()
        {
            LeagueNaming.Find(Entries, "TauronLigaKobiet", 2020).ShouldBeNull();
        }

        [Fact]
        public void Validate_Should_ReturnNoErrors_When_TheEntriesAreValid()
        {
            LeagueNaming.Validate(Entries, LeagueKeys).ShouldBeEmpty();
        }

        [Fact]
        public void Validate_Should_ReportAnUnknownLeague()
        {
            LeagueNameFile[] entries = [new("Plusliga", 2007, "PlusLiga")];

            LeagueNaming.Validate(entries, LeagueKeys).ShouldBe(["Entry 0: unknown league 'Plusliga'."]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_Should_ReportABlankName(string? name)
        {
            LeagueNameFile[] entries = [new("PlusLiga", 2007, name!)];

            LeagueNaming.Validate(entries, LeagueKeys).ShouldBe(["Entry 0: the name must have 1 to 100 characters."]);
        }

        [Fact]
        public void Validate_Should_ReportANameLongerThanTheColumn()
        {
            LeagueNameFile[] entries = [new("PlusLiga", 2007, new string('a', LeagueNaming.MaxNameLength + 1))];

            LeagueNaming.Validate(entries, LeagueKeys).ShouldBe(["Entry 0: the name must have 1 to 100 characters."]);
        }

        [Fact]
        public void Validate_Should_ReportTwoEntriesOfOneLeagueForTheSameYear()
        {
            LeagueNameFile[] entries = [new("PlusLiga", 2007, "PlusLiga"), new("PlusLiga", 2007, "Plus Liga")];

            LeagueNaming.Validate(entries, LeagueKeys).ShouldBe(["League 'PlusLiga' has 2 entries from 2007."]);
        }
    }
}
