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
        public async Task OpenApiDocumentDescribesTheLeaguesList()
        {
            var json = await Client.GetStringAsync(new Uri("/openapi/v1.json", UriKind.Relative), TestContext.Current.CancellationToken);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            root.GetProperty("info").GetProperty("description").GetString().ShouldNotBeNullOrWhiteSpace();

            var operation = root.GetProperty("paths").GetProperty("/api/v1/leagues").GetProperty("get");
            operation.GetProperty("summary").GetString().ShouldBe("List leagues");

            var parameters = operation.GetProperty("parameters").EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            parameters.Keys.ShouldBe(["gender", "division", "sort", "offset", "limit"], ignoreOrder: true);
            foreach (var parameter in parameters.Values)
            {
                parameter.GetProperty("description").GetString().ShouldNotBeNullOrWhiteSpace();
            }

            parameters["gender"].GetProperty("description").GetString().ShouldBe("Gender id: 1 = Men, 2 = Women.");
            parameters["sort"].GetProperty("description").GetString().ShouldNotBeNull()
                .ShouldContain("Fields: name, divisionNumber. Default: name.", Case.Sensitive);

            var schemas = root.GetProperty("components").GetProperty("schemas");
            var page = schemas.GetProperty("PagedResponseOfGetLeaguesResponse").GetProperty("properties");
            page.TryGetProperty("links", out _).ShouldBeTrue();
            var links = schemas.GetProperty("PageLinks").GetProperty("properties");
            links.EnumerateObject().Select(property => property.Name).ShouldBe(["self", "first", "prev", "next", "last"], ignoreOrder: true);
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
