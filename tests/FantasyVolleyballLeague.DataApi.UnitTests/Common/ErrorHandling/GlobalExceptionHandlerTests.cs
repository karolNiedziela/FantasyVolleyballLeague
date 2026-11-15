using FantasyVolleyballLeague.DataApi.Common.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.ErrorHandling
{
    public sealed class GlobalExceptionHandlerTests
    {
        private readonly IProblemDetailsService _problemDetailsService = Substitute.For<IProblemDetailsService>();
        private readonly IHostEnvironment _environment = Substitute.For<IHostEnvironment>();
        private readonly DefaultHttpContext _httpContext = new();

        public GlobalExceptionHandlerTests()
        {
            _environment.EnvironmentName.Returns(Environments.Production);
        }

        [Theory]
        [InlineData(StatusCodes.Status400BadRequest)]
        [InlineData(StatusCodes.Status408RequestTimeout)]
        [InlineData(StatusCodes.Status413PayloadTooLarge)]
        public async Task TryHandleAsync_Should_KeepTheStatusCodeOfABadHttpRequestException(int statusCode)
        {
            var exception = new BadHttpRequestException("The request is not valid.", statusCode);

            var handled = await CreateHandler().TryHandleAsync(_httpContext, exception, TestContext.Current.CancellationToken);

            handled.ShouldBeTrue();
            _httpContext.Response.StatusCode.ShouldBe(statusCode);
            var problem = WrittenProblem();
            problem.Status.ShouldBe(statusCode);
            problem.Title.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task TryHandleAsync_Should_UseTheMessageOfABadHttpRequestExceptionAsDetail_InEveryEnvironment()
        {
            var exception = new BadHttpRequestException("Failed to read parameter \"int Offset\".", StatusCodes.Status400BadRequest);

            await CreateHandler().TryHandleAsync(_httpContext, exception, TestContext.Current.CancellationToken);

            WrittenProblem().Detail.ShouldBe("Failed to read parameter \"int Offset\".");
        }

        [Fact]
        public async Task TryHandleAsync_Should_ReturnInternalServerErrorWithoutDetail_When_AnUnexpectedExceptionIsThrownInProduction()
        {
            var exception = new InvalidOperationException("secret internals");

            await CreateHandler().TryHandleAsync(_httpContext, exception, TestContext.Current.CancellationToken);

            _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
            var problem = WrittenProblem();
            problem.Status.ShouldBe(StatusCodes.Status500InternalServerError);
            problem.Detail.ShouldBeNull();
        }

        [Fact]
        public async Task TryHandleAsync_Should_ReturnTheExceptionMessageAsDetail_When_AnUnexpectedExceptionIsThrownInDevelopment()
        {
            _environment.EnvironmentName.Returns(Environments.Development);
            var exception = new InvalidOperationException("something broke");

            await CreateHandler().TryHandleAsync(_httpContext, exception, TestContext.Current.CancellationToken);

            WrittenProblem().Detail.ShouldBe("something broke");
        }

        private GlobalExceptionHandler CreateHandler()
        {
            return new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, _problemDetailsService, _environment);
        }

        private ProblemDetails WrittenProblem()
        {
            var call = _problemDetailsService.ReceivedCalls().Single();

            return ((ProblemDetailsContext)call.GetArguments()[0]!).ProblemDetails;
        }
    }
}
