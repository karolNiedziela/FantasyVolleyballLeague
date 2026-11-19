using FantasyVolleyballLeague.DataApi.Common.API.Lookups;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Lookups
{
    public sealed class CodeValidationFilterTests
    {
        private const string Parameter = "gender";

        private static readonly object NextResult = new();

        [Theory]
        [InlineData("")]
        [InlineData("?gender=men")]
        [InlineData("?gender=men&gender=women")]
        [InlineData("?division=2")]
        public async Task InvokeAsync_Should_CallTheEndpoint_When_EveryValueIsAKnownCodeOrTheParameterIsMissing(string queryString)
        {
            var result = await Invoke(queryString);

            result.ShouldBeSameAs(NextResult);
        }

        [Theory]
        [InlineData("?gender=female", "female")]
        [InlineData("?gender=Men", "Men")]
        [InlineData("?gender=", "")]
        public async Task InvokeAsync_Should_ReturnAValidationProblem_When_TheValueIsNotAKnownCode(string queryString, string value)
        {
            var result = await Invoke(queryString);

            var problem = result.ShouldBeOfType<ValidationProblem>();
            problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            var error = problem.ProblemDetails.Errors[Parameter].ShouldHaveSingleItem();
            error.ShouldContain($"'{value}'");
            error.ShouldContain("men, women");
        }

        [Fact]
        public async Task InvokeAsync_Should_ReportEveryUnknownValue_When_SeveralAreGiven()
        {
            var result = await Invoke("?gender=men&gender=x&gender=y");

            var problem = result.ShouldBeOfType<ValidationProblem>();
            problem.ProblemDetails.Errors[Parameter].Length.ShouldBe(2);
        }

        private static async Task<object?> Invoke(string queryString)
        {
            var filter = new CodeValidationFilter(Parameter, ["men", "women"]);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString(queryString);
            var context = new DefaultEndpointFilterInvocationContext(httpContext);

            return await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(NextResult));
        }
    }
}
