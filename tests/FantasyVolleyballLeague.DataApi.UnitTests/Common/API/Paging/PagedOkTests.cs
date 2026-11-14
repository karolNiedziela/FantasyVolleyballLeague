using System.Text.Json;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Paging
{
    public sealed class PagedOkTests
    {
        [Fact]
        public async Task ExecuteAsyncWritesTheLinksFromTheRequestUrlInTheBody()
        {
            var httpContext = CreateHttpContext("/api/v1/leagues", "?sort=name&offset=25&limit=25");
            var result = PagedResults.Ok(new PagedResponse<int>([], offset: 25, limit: 25, totalCount: 100));

            await result.ExecuteAsync(httpContext);

            var links = (await ReadBody(httpContext.Response)).GetProperty("links");
            links.GetProperty("self").GetString().ShouldBe("https://api.example.test/api/v1/leagues?sort=name&offset=25&limit=25");
            links.GetProperty("first").GetString().ShouldBe("https://api.example.test/api/v1/leagues?sort=name&offset=0&limit=25");
            links.GetProperty("prev").GetString().ShouldBe("https://api.example.test/api/v1/leagues?sort=name&offset=0&limit=25");
            links.GetProperty("next").GetString().ShouldBe("https://api.example.test/api/v1/leagues?sort=name&offset=50&limit=25");
            links.GetProperty("last").GetString().ShouldBe("https://api.example.test/api/v1/leagues?sort=name&offset=75&limit=25");
        }

        [Fact]
        public async Task ExecuteAsyncWritesNullForPrevAndNextOnASinglePage()
        {
            var httpContext = CreateHttpContext("/api/v1/leagues", string.Empty);
            var result = PagedResults.Ok(new PagedResponse<int>([7], offset: 0, limit: 25, totalCount: 1));

            await result.ExecuteAsync(httpContext);

            var links = (await ReadBody(httpContext.Response)).GetProperty("links");
            links.GetProperty("prev").ValueKind.ShouldBe(JsonValueKind.Null);
            links.GetProperty("next").ValueKind.ShouldBe(JsonValueKind.Null);
            links.GetProperty("self").GetString().ShouldBe("https://api.example.test/api/v1/leagues?offset=0&limit=25");
        }

        [Fact]
        public async Task ExecuteAsyncKeepsThePathBaseAndThePort()
        {
            var httpContext = CreateHttpContext("/leagues", "?offset=0&limit=5");
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost", 5080);
            httpContext.Request.PathBase = "/api/v1";
            var result = PagedResults.Ok(new PagedResponse<int>([], offset: 0, limit: 5, totalCount: 20));

            await result.ExecuteAsync(httpContext);

            var next = (await ReadBody(httpContext.Response)).GetProperty("links").GetProperty("next");
            next.GetString().ShouldBe("http://localhost:5080/api/v1/leagues?offset=5&limit=5");
        }

        [Fact]
        public async Task ExecuteAsyncWritesStatus200AndThePageAsJson()
        {
            var httpContext = CreateHttpContext("/api/v1/leagues", string.Empty);
            var result = PagedResults.Ok(new PagedResponse<int>([7, 8], offset: 0, limit: 25, totalCount: 2));

            await result.ExecuteAsync(httpContext);

            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
            httpContext.Response.ContentType.ShouldNotBeNull().ShouldStartWith("application/json");
            var body = await ReadBody(httpContext.Response);
            body.GetProperty("items").EnumerateArray().Select(item => item.GetInt32()).ShouldBe([7, 8]);
            body.GetProperty("offset").GetInt32().ShouldBe(0);
            body.GetProperty("limit").GetInt32().ShouldBe(25);
            body.GetProperty("totalCount").GetInt32().ShouldBe(2);
        }

        [Fact]
        public async Task ExecuteAsyncWritesTheLinksInsideAResultsUnion()
        {
            var httpContext = CreateHttpContext("/api/v1/leagues", string.Empty);
            Results<PagedOk<int>, NotFound> result = PagedResults.Ok(new PagedResponse<int>([], offset: 0, limit: 25, totalCount: 60));

            await result.ExecuteAsync(httpContext);

            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
            var next = (await ReadBody(httpContext.Response)).GetProperty("links").GetProperty("next");
            next.GetString().ShouldBe("https://api.example.test/api/v1/leagues?offset=25&limit=25");
        }

        [Fact]
        public void ResultExposesThePageAndTheStatusCode()
        {
            var page = new PagedResponse<int>([], offset: 0, limit: 25, totalCount: 0);

            var result = PagedResults.Ok(page);

            result.Value.ShouldBeSameAs(page);
            ((IStatusCodeHttpResult)result).StatusCode.ShouldBe(StatusCodes.Status200OK);
        }

        private static DefaultHttpContext CreateHttpContext(string path, string queryString)
        {
            var services = new ServiceCollection().AddLogging().BuildServiceProvider();

            var httpContext = new DefaultHttpContext { RequestServices = services };
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("api.example.test");
            httpContext.Request.Path = path;
            httpContext.Request.QueryString = new QueryString(queryString);
            httpContext.Response.Body = new MemoryStream();

            return httpContext;
        }

        private static async Task<JsonElement> ReadBody(HttpResponse response)
        {
            response.Body.Position = 0;
            using var document = await JsonDocument.ParseAsync(response.Body);

            return document.RootElement.Clone();
        }
    }
}
