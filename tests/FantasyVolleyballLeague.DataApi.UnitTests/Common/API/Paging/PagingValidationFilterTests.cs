using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Paging
{
    public sealed class PagingValidationFilterTests
    {
        private static readonly object NextResult = new();

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, PagingDefaults.DefaultLimit)]
        [InlineData(1000, PagingDefaults.MaxLimit)]
        public async Task InvokeAsyncCallsTheEndpointWhenThePagingIsValid(int offset, int limit)
        {
            var result = await Invoke(new PageQuery(offset, limit));

            result.ShouldBeSameAs(NextResult);
        }

        [Fact]
        public async Task InvokeAsyncRejectsANegativeOffset()
        {
            var result = await Invoke(new PageQuery(-1, PagingDefaults.DefaultLimit));

            var errors = ErrorsOf(result);
            errors.Keys.ShouldBe(["offset"]);
            errors["offset"].ShouldHaveSingleItem().ShouldContain("0 or greater");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(PagingDefaults.MaxLimit + 1)]
        public async Task InvokeAsyncRejectsALimitOutsideTheRange(int limit)
        {
            var result = await Invoke(new PageQuery(0, limit));

            var errors = ErrorsOf(result);
            errors.Keys.ShouldBe(["limit"]);
            errors["limit"].ShouldHaveSingleItem().ShouldContain($"between 1 and {PagingDefaults.MaxLimit}");
        }

        [Fact]
        public async Task InvokeAsyncReportsAnInvalidOffsetAndLimitTogether()
        {
            var result = await Invoke(new PageQuery(-1, 0));

            ErrorsOf(result).Keys.OrderBy(key => key).ShouldBe(["limit", "offset"]);
        }

        [Fact]
        public async Task InvokeAsyncThrowsWhenThereIsNoPagedArgument()
        {
            var exception = await Should.ThrowAsync<InvalidOperationException>(() => Invoke("not a query"));

            exception.Message.ShouldContain(nameof(IPagedQuery));
        }

        private static IDictionary<string, string[]> ErrorsOf(object? result)
        {
            var problem = result.ShouldBeOfType<ValidationProblem>();
            problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);

            return problem.ProblemDetails.Errors;
        }

        private static async Task<object?> Invoke(object argument)
        {
            var filter = new PagingValidationFilter();
            var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), argument);

            return await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(NextResult));
        }

        private sealed record PageQuery(int Offset, int Limit) : IPagedQuery;
    }
}
