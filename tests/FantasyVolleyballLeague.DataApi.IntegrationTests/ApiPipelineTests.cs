using System.Net;
using System.Net.Mime;
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

            await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UnknownRouteReturnsAProblemDetailsNotFound()
        {
            var response = await Client.GetAsync(new Uri("/api/v1/does-not-exist", UriKind.Relative), TestContext.Current.CancellationToken);

            await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
            response.ShouldHaveMediaType(MediaTypeNames.Application.ProblemJson);
        }
    }
}
