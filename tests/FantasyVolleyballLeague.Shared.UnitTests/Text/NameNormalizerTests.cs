using FantasyVolleyballLeague.Shared.Text;

namespace FantasyVolleyballLeague.Shared.UnitTests.Text
{
    public sealed class NameNormalizerTests
    {
        [Theory]
        [InlineData("Łukasz Kaczmarek", "lukasz kaczmarek")]
        [InlineData("KACZMAREK ŁUKASZ", "kaczmarek lukasz")]
        [InlineData("Jakub Szymański", "jakub szymanski")]
        [InlineData("Straßer", "strasser")]
        [InlineData("Søren Ærø", "soren aero")]
        [InlineData("Đorđe Œuvre", "dorde oeuvre")]
        [InlineData("Þór Ðóra ısı", "thor dora isi")]
        [InlineData("Rybak-Czyrniańska", "rybak czyrnianska")]
        [InlineData("O'Brien", "obrien")]
        [InlineData("O’Brien", "obrien")]
        [InlineData("O`Brien", "obrien")]
        [InlineData("O´Brien", "obrien")]
        [InlineData("ASSECO RESOVIA S.S.A.", "asseco resovia ssa")]
        [InlineData("  Jan   Kowalski\t", "jan kowalski")]
        [InlineData("Jan Kowalski", "jan kowalski")]
        public void Normalize_Should_ApplyTheRules(string text, string expected)
        {
            NameNormalizer.Normalize(text).ShouldBe(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("-- . '")]
        public void Normalize_Should_ReturnEmpty_When_NothingSearchableIsLeft(string text)
        {
            NameNormalizer.Normalize(text).ShouldBeEmpty();
            NameNormalizer.Tokens(text).ShouldBeEmpty();
        }

        [Theory]
        [InlineData("Łukasz Kaczmarek")]
        [InlineData("Rybak-Czyrniańska")]
        [InlineData("ASSECO RESOVIA S.S.A.")]
        [InlineData("Straßer Søren")]
        public void Normalize_Should_BeIdempotent(string text)
        {
            var once = NameNormalizer.Normalize(text);

            NameNormalizer.Normalize(once).ShouldBe(once);
        }

        [Fact]
        public void Tokens_Should_SplitOnHyphenAndKeepTheOrder()
        {
            NameNormalizer.Tokens("Anna Rybak-Czyrniańska").ShouldBe(["anna", "rybak", "czyrnianska"]);
        }
    }
}
