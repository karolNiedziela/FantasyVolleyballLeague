using System.Reflection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Metadata;

namespace FantasyVolleyballLeague.DataApi.Common.API.Paging
{
    internal sealed class PagedOk<T> : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<PagedResponse<T>>
    {
        public PagedOk(PagedResponse<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);

            Value = value;
        }

        public PagedResponse<T> Value { get; }

        object? IValueHttpResult.Value => Value;

        int? IStatusCodeHttpResult.StatusCode => StatusCodes.Status200OK;

        public Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var links = PageLinks.Create(httpContext.Request.GetEncodedUrl(), Value.Offset, Value.Limit, Value.TotalCount);

            return TypedResults.Ok(Value.WithLinks(links)).ExecuteAsync(httpContext);
        }

        static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Metadata.Add(new ProducesResponseTypeMetadata(
                StatusCodes.Status200OK, typeof(PagedResponse<T>), ["application/json"]));
        }
    }
}
