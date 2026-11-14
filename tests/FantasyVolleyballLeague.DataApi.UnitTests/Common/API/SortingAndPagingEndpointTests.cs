using System.Text.Json;
using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API
{
    public sealed class SortingAndPagingEndpointTests
    {
        [Fact]
        public async Task ValidRequestReachesTheEndpointAndGetsLinksInTheBody()
        {
            await using var app = CreateApp(route => route.WithSorting<ItemQuery, Item>().WithPaging());

            var response = await Send(app, "?sort=-name&offset=25&limit=25");

            response.StatusCode.ShouldBe(StatusCodes.Status200OK);
            using var body = JsonDocument.Parse(await ReadBody(response));
            body.RootElement.GetProperty("links").GetProperty("next").GetString().ShouldNotBeNull().ShouldContain("offset=50&limit=25");
        }

        [Fact]
        public async Task InvalidSortReturnsAValidationProblem()
        {
            await using var app = CreateApp(route => route.WithSorting<ItemQuery, Item>().WithPaging());

            var response = await Send(app, "?sort=height");

            response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            (await ReadBody(response)).ShouldContain("Unknown sort field 'height'");
        }

        [Fact]
        public async Task InvalidLimitReturnsAValidationProblem()
        {
            await using var app = CreateApp(route => route.WithSorting<ItemQuery, Item>().WithPaging());

            var response = await Send(app, $"?limit={PagingDefaults.MaxLimit + 1}");

            response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            (await ReadBody(response)).ShouldContain("The limit must be between 1 and");
        }

        [Fact]
        public async Task BothExtensionsDocumentTheValidationProblemOnce()
        {
            await using var app = CreateApp(route => route.WithSorting<ItemQuery, Item>().WithPaging());

            var responses = ValidationProblemResponses(app);

            responses.ShouldHaveSingleItem().Type.ShouldBe(typeof(HttpValidationProblemDetails));
        }

        [Fact]
        public async Task PagedOkDocumentsThePageAsThe200Response()
        {
            await using var app = CreateApp(route => route.WithPaging());

            var okResponse = Endpoint(app).Metadata
                .OfType<IProducesResponseTypeMetadata>()
                .Where(metadata => metadata.StatusCode == StatusCodes.Status200OK)
                .ShouldHaveSingleItem();

            okResponse.Type.ShouldBe(typeof(PagedResponse<string>));
            okResponse.ContentTypes.ShouldBe(["application/json"]);
        }

        [Fact]
        public async Task EachExtensionDocumentsTheValidationProblemOnItsOwn()
        {
            await using var sortingOnly = CreateApp(route => route.WithSorting<ItemQuery, Item>());
            await using var pagingOnly = CreateApp(route => route.WithPaging());

            ValidationProblemResponses(sortingOnly).ShouldHaveSingleItem();
            ValidationProblemResponses(pagingOnly).ShouldHaveSingleItem();
        }

        private static WebApplication CreateApp(Action<RouteHandlerBuilder> configure)
        {
            var app = WebApplication.CreateBuilder().Build();

            var route = app.MapGet("/items", ([AsParameters] ItemQuery query) => PagedResults.Ok(new PagedResponse<string>(["item"], query.Offset, query.Limit, totalCount: 100)));
            configure(route);

            return app;
        }

        private static RouteEndpoint Endpoint(WebApplication app)
        {
            return ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(source => source.Endpoints)
                .OfType<RouteEndpoint>()
                .Single();
        }

        private static List<IProducesResponseTypeMetadata> ValidationProblemResponses(WebApplication app)
        {
            return Endpoint(app).Metadata
                .OfType<IProducesResponseTypeMetadata>()
                .Where(metadata => metadata.StatusCode == StatusCodes.Status400BadRequest)
                .ToList();
        }

        private static async Task<HttpResponse> Send(WebApplication app, string queryString)
        {
            var endpoint = Endpoint(app);
            using var scope = app.Services.CreateScope();

            var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            context.Request.Method = HttpMethods.Get;
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            context.Request.Path = "/items";
            context.Request.QueryString = new QueryString(queryString);
            context.Response.Body = new MemoryStream();

            await endpoint.RequestDelegate!(context);

            return context.Response;
        }

        private static async Task<string> ReadBody(HttpResponse response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);

            return await reader.ReadToEndAsync();
        }

        private sealed class Item : Entity
        {
            public string Name { get; } = string.Empty;
        }

        private sealed record ItemQuery(
            string? Sort,
            int Offset = PagingDefaults.DefaultOffset,
            int Limit = PagingDefaults.DefaultLimit) : ISortableQuery<Item>, IPagedQuery
        {
            public static SortMap<Item> SortMap { get; } = new SortMap<Item>()
                .Add("name", item => item.Name)
                .WithDefault("name");
        }
    }
}
