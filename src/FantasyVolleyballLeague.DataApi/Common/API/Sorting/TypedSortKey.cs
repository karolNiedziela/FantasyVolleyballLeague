using System.Linq.Expressions;

namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal sealed class TypedSortKey<TSource, TKey> : ISortKey<TSource>
    {
        private readonly Expression<Func<TSource, TKey>> _keySelector;

        public TypedSortKey(Expression<Func<TSource, TKey>> keySelector)
        {
            _keySelector = keySelector;
        }

        public IOrderedQueryable<TSource> OrderBy(IQueryable<TSource> source, bool ascending)
        {
            return ascending ? source.OrderBy(_keySelector) : source.OrderByDescending(_keySelector);
        }

        public IOrderedQueryable<TSource> ThenBy(IOrderedQueryable<TSource> source, bool ascending)
        {
            return ascending ? source.ThenBy(_keySelector) : source.ThenByDescending(_keySelector);
        }
    }
}
