namespace FantasyVolleyballLeague.DataApi.Common.API.Sorting
{
    internal readonly record struct SortInstruction<T>(ISortKey<T> Key, bool Ascending);
}
