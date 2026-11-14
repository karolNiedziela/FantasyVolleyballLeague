namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal interface ISortKey<TSource>
    {
        IOrderedQueryable<TSource> OrderBy(IQueryable<TSource> source, bool ascending);

        IOrderedQueryable<TSource> ThenBy(IOrderedQueryable<TSource> source, bool ascending);
    }
}