using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.UnitTests.Fixtures;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API
{
    public sealed class SortingAndPagingEndpointTests
    {
        [Fact]
        public async Task BothExtensionsDocumentTheValidationProblemOnce()
        {
            await using var app = CreateApp(route => route.WithSorting<ItemQuery, SortItem>().WithPaging());

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
            await using var sortingOnly = CreateApp(route => route.WithSorting<ItemQuery, SortItem>());
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

        private sealed record ItemQuery(
            string? Sort,
            int Offset = PagingDefaults.DefaultOffset,
            int Limit = PagingDefaults.DefaultLimit) : ISortableQuery<SortItem>, IPagedQuery
        {
            public static SortMap<SortItem> SortMap { get; } = new SortMap<SortItem>()
                .Add("name", item => item.Name)
                .WithDefault("name");
        }
    }
}
