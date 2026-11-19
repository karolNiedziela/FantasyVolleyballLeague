using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.UnitTests.Fixtures;
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
            var result = await Invoke(new SortItemQuery(sort));

            result.ShouldBeSameAs(NextResult);
        }

        [Fact]
        public async Task InvokeAsyncReturnsAValidationProblemWhenTheSortIsInvalid()
        {
            var result = await Invoke(new SortItemQuery("name,height"));

            var problem = result.ShouldBeOfType<ValidationProblem>();
            problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            problem.ProblemDetails.Errors["sort"].ShouldHaveSingleItem().ShouldContain("'height'");
        }

        [Fact]
        public async Task InvokeAsyncThrowsWhenThereIsNoSortableArgument()
        {
            var exception = await Should.ThrowAsync<InvalidOperationException>(() => Invoke("not a query"));

            exception.Message.ShouldContain(nameof(SortItemQuery));
        }

        private static async Task<object?> Invoke(object argument)
        {
            var filter = new SortValidationFilter<SortItemQuery, SortItem>();
            var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), argument);

            return await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(NextResult));
        }
    }
}
