using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Sorting
{
    public sealed class SortMapTests
    {
        private static readonly SortMap<Item> Map = new SortMap<Item>()
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
            var result = Sorted("name").ToList();

            var tied = result.Where(item => item.Name == "b").Select(item => item.Id).ToList();
            tied.ShouldBe(tied.OrderBy(id => id).ToList());
        }

        [Fact]
        public void ParseReportsAnUnknownField()
        {
            var result = Map.Parse("name,height");

            result.IsValid.ShouldBeFalse();
            result.Errors["sort"].ShouldHaveSingleItem().ShouldContain("'height'");
        }

        [Theory]
        [InlineData("name,,rank")]
        [InlineData("-")]
        [InlineData("name,-name")]
        public void ParseReportsEmptyAndDuplicateFields(string sort)
        {
            Map.Parse(sort).IsValid.ShouldBeFalse();
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
            var map = new SortMap<Item>()
                .Add("name", item => item.Name)
                .Add("rank", item => item.Rank);

            var exception = Should.Throw<ArgumentException>(() => map.WithDefault(sort));

            exception.Message.ShouldContain(expectedMessage);
        }

        [Fact]
        public void WithDefaultAcceptsDescendingAndSecondaryFields()
        {
            var map = new SortMap<Item>()
                .Add("name", item => item.Name)
                .Add("rank", item => item.Rank)
                .WithDefault("-name,rank");

            var result = map.Parse(null).Apply(Items().AsQueryable()).ToList();

            result.Select(item => (item.Name, item.Rank)).ShouldBe([("c", 1), ("b", 2), ("b", 9), ("a", 5)]);
        }

        private static IQueryable<Item> Sorted(string? sort)
        {
            var parsed = Map.Parse(sort);
            parsed.IsValid.ShouldBeTrue();

            return parsed.Apply(Items().AsQueryable());
        }

        private static List<string> Names(IQueryable<Item> query)
        {
            return query.Select(item => item.Name).ToList();
        }

        private static List<Item> Items()
        {
            return
            [
                new Item("b", 9),
                new Item("c", 1),
                new Item("a", 5),
                new Item("b", 2),
            ];
        }

        private sealed class Item : Entity
        {
            public Item(string name, int rank)
            {
                Name = name;
                Rank = rank;
            }

            public string Name { get; }

            public int Rank { get; }
        }
    }
}
