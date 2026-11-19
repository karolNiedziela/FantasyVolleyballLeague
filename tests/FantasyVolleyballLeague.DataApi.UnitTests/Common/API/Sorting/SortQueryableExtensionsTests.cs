using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.UnitTests.Fixtures;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Sorting
{
    public sealed class SortQueryableExtensionsTests
    {
        [Fact]
        public void ApplySort_Should_OrderByTheSortOfTheQuery()
        {
            var items = new[] { new SortItem("a"), new SortItem("b") };

            var result = items.AsQueryable().ApplySort(new SortItemQuery("-name")).Select(item => item.Name).ToList();

            result.ShouldBe(["b", "a"]);
        }
    }
}
