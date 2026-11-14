using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Sorting
{
    public sealed class SortValidationFilterTests
    {
        private static readonly object NextResult = new();

        [Theory]
        [InlineData(null)]
        [InlineData("-name,rank")]
        public async Task InvokeAsyncCallsTheEndpointWhenTheSortIsValid(string? sort)
        {
            var result = await Invoke(new ItemQuery(sort));

            result.ShouldBeSameAs(NextResult);
        }

        [Fact]
        public async Task InvokeAsyncReturnsAValidationProblemWhenTheSortIsInvalid()
        {
            var result = await Invoke(new ItemQuery("name,height"));

            var problem = result.ShouldBeOfType<ValidationProblem>();
            problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            problem.ProblemDetails.Errors["sort"].ShouldHaveSingleItem().ShouldContain("'height'");
        }

        [Fact]
        public async Task InvokeAsyncThrowsWhenThereIsNoSortableArgument()
        {
            var exception = await Should.ThrowAsync<InvalidOperationException>(() => Invoke("not a query"));

            exception.Message.ShouldContain(nameof(ItemQuery));
        }

        [Fact]
        public void ApplySortOrdersByTheQuerySort()
        {
            var items = new[] { new Item("b", 1), new Item("a", 2) };

            var result = items.AsQueryable().ApplySort(new ItemQuery("-name")).Select(item => item.Name).ToList();

            result.ShouldBe(["b", "a"]);
        }

        private static async Task<object?> Invoke(object argument)
        {
            var filter = new SortValidationFilter<ItemQuery, Item>();
            var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), argument);

            return await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(NextResult));
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

        private sealed record ItemQuery(string? Sort) : ISortableQuery<Item>
        {
            public static SortMap<Item> SortMap { get; } = new SortMap<Item>()
                .Add("name", item => item.Name)
                .Add("rank", item => item.Rank)
                .WithDefault("name");
        }
    }
}
