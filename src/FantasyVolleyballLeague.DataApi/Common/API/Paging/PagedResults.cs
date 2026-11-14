namespace FantasyVolleyballLeague.DataApi.Common.API.Paging
{
    internal static class PagedResults
    {
        public static PagedOk<T> Ok<T>(PagedResponse<T> page)
        {
            return new PagedOk<T>(page);
        }
    }
}
