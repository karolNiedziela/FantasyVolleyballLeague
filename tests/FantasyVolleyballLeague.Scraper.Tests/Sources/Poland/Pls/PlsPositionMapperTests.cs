using FantasyVolleyballLeague.Contracts;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsPositionMapperTests
    {
        [Theory]
        [InlineData("Przyjmujący", PositionNames.OutsideHitter)]
        [InlineData("Przyjmująca", PositionNames.OutsideHitter)]
        [InlineData("Atakujący", PositionNames.OppositeHitter)]
        [InlineData("Atakująca", PositionNames.OppositeHitter)]
        [InlineData("Środkowy", PositionNames.MiddleBlocker)]
        [InlineData("Środkowa", PositionNames.MiddleBlocker)]
        [InlineData("Rozgrywający", PositionNames.Setter)]
        [InlineData("Rozgrywająca", PositionNames.Setter)]
        [InlineData("Libero", PositionNames.Libero)]
        public void Map_Should_TranslateThePolishPosition(string polish, string expected)
        {
            PlsPositionMapper.Map(polish).ShouldBe(expected);
        }

        [Theory]
        [InlineData("  ATAKUJĄCY ", PositionNames.OppositeHitter)]
        [InlineData("libero", PositionNames.Libero)]
        public void Map_Should_IgnoreCaseAndSurroundingWhitespace(string polish, string expected)
        {
            PlsPositionMapper.Map(polish).ShouldBe(expected);
        }

        [Fact]
        public void Map_Should_ReturnTheTrimmedInput_When_ThePositionIsUnknown()
        {
            PlsPositionMapper.Map("Trener ").ShouldBe("Trener");
        }
    }
}
