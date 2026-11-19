using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.UnitTests.Fixtures;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Sorting
{
    public sealed class SortMapTests
    {
        private static readonly SortMap<SortItem> Map = new SortMap<SortItem>()
            .Add("name", item => item.Name)
            .Add("rank", item => item.Rank)
            .WithDefault("name");

        [Fact]
        public void FieldsListsTheRegisteredSortKeys()
        {
            Map.Fields.ShouldBe(["name", "rank"], ignoreOrder: true);
        }

        [Fact]
        public void DefaultSortIsTheConfiguredDefault()
        {
            Map.DefaultSort.ShouldBe("name");
        }

        [Fact]
        public void ParseOrdersByTheDefaultWhenNoSortIsGiven()
        {
            var result = Names(Sorted(null));

            result.ShouldBe(["a", "b", "b", "c"]);
        }

        [Fact]
        public void ParseUsesDescendingPrefixAndSecondaryFields()
        {
            var result = Sorted("-name,-rank").ToList();

            result.Select(item => (item.Name, item.Rank)).ShouldBe([("c", 1), ("b", 9), ("b", 2), ("a", 5)]);
        }

        [Fact]
        public void ParseIsCaseInsensitiveAboutFieldNames()
        {
            var result = Names(Sorted("-NAME"));

            result.ShouldBe(["c", "b", "b", "a"]);
        }

        [Fact]
        public void ApplyBreaksTiesById()
        {
            var first = new SortItem("b", 1);
            var second = new SortItem("b", 2);
            var (lower, higher) = first.Id < second.Id ? (first, second) : (second, first);
            var items = new List<SortItem> { higher, lower };

            var result = Map.Parse("name").Apply(items.AsQueryable()).Select(item => item.Id).ToList();

            result.ShouldBe([lower.Id, higher.Id]);
        }

        [Fact]
        public void ParseReportsAnUnknownField()
        {
            var result = Map.Parse("name,height");

            result.IsValid.ShouldBeFalse();
            result.Errors["sort"].ShouldHaveSingleItem().ShouldContain("'height'");
        }

        [Theory]
        [InlineData("name,,rank", "must not be empty")]
        [InlineData("-", "must not be empty")]
        [InlineData("name,-name", "listed more than once")]
        public void Parse_Should_ReportAnEmptyOrDuplicateField(string sort, string expectedMessage)
        {
            var result = Map.Parse(sort);

            result.IsValid.ShouldBeFalse();
            result.Errors["sort"].ShouldHaveSingleItem().ShouldContain(expectedMessage);
        }

        [Fact]
        public void ApplyRejectsAnInvalidSort()
        {
            var result = Map.Parse("height");

            Should.Throw<InvalidOperationException>(() => result.Apply(Items().AsQueryable()));
        }

        [Theory]
        [InlineData("-height", "Unknown sort field 'height'")]
        [InlineData("--name", "Unknown sort field '-name'")]
        [InlineData("name,,rank", "must not be empty")]
        [InlineData("", "must not be empty")]
        [InlineData("-", "must not be empty")]
        [InlineData("name,-NAME", "listed more than once")]
        public void WithDefaultRejectsADefaultThatParseWouldReject(string sort, string expectedMessage)
        {
            var map = new SortMap<SortItem>()
                .Add("name", item => item.Name)
                .Add("rank", item => item.Rank);

            var exception = Should.Throw<ArgumentException>(() => map.WithDefault(sort));

            exception.Message.ShouldContain(expectedMessage);
        }

        [Fact]
        public void WithDefaultAcceptsDescendingAndSecondaryFields()
        {
            var map = new SortMap<SortItem>()
                .Add("name", item => item.Name)
                .Add("rank", item => item.Rank)
                .WithDefault("-name,rank");

            var result = map.Parse(null).Apply(Items().AsQueryable()).ToList();

            result.Select(item => (item.Name, item.Rank)).ShouldBe([("c", 1), ("b", 2), ("b", 9), ("a", 5)]);
        }

        private static IQueryable<SortItem> Sorted(string? sort)
        {
            var parsed = Map.Parse(sort);
            parsed.IsValid.ShouldBeTrue();

            return parsed.Apply(Items().AsQueryable());
        }

        private static List<string> Names(IQueryable<SortItem> query)
        {
            return query.Select(item => item.Name).ToList();
        }

        private static List<SortItem> Items()
        {
            return
            [
                new SortItem("b", 9),
                new SortItem("c", 1),
                new SortItem("a", 5),
                new SortItem("b", 2),
            ];
        }

    }
}
