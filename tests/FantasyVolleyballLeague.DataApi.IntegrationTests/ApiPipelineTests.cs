using System.Net;
using System.Text.Json;
using FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests
{
    public sealed class ApiPipelineTests : IntegrationTestBase
    {
        public ApiPipelineTests(DataApiFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task OpenApiDocumentOfVersion1IsServed()
        {
            var response = await Client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative), TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UnknownRouteReturnsAProblemDetailsNotFound()
        {
            var response = await Client.GetAsync(new Uri("/api/v1/does-not-exist", UriKind.Relative), TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        }
    }
}
