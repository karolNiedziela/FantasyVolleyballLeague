using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class PlayerNameKeyTests
    {
        [Theory]
        [InlineData("Benjamin Toniutti", "Toniutti Benjamin")]
        [InlineData("Łukasz Kaczmarek", "Kaczmarek Łukasz")]
        [InlineData("Łukasz Kaczmarek", "KACZMAREK LUKASZ")]
        [InlineData("Bartłomiej Krulicki", "krulicki  bartlomiej")]
        [InlineData("Tyler James Sanders", "Sanders Tyler James")]
        [InlineData("Jakub Szymański", "Szymanski Jakub")]
        public void CreateIgnoresOrderCaseAndDiacritics(string first, string second)
        {
            Assert.Equal(PlayerNameKey.Create(first), PlayerNameKey.Create(second));
        }

        [Fact]
        public void CreateDistinguishesDifferentPlayers()
        {
            Assert.NotEqual(PlayerNameKey.Create("Jakub Szymański"), PlayerNameKey.Create("Jakub Kochanowski"));
        }

        [Theory]
        [InlineData("Kaczmarek Łukasz", "Kaczmarek Lukas", true)]
        [InlineData("Kaczmarek Łukasz", "Kochanowski Jakub", false)]
        public void ShareTokenDetectsCommonSurname(string first, string second, bool expected)
        {
            Assert.Equal(expected, PlayerNameKey.ShareToken(PlayerNameKey.Create(first), PlayerNameKey.Create(second)));
        }
    }
}
