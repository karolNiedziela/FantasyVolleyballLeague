namespace FantasyVolleyballLeague.DataApi.Common.API.Paging
{
    internal interface IPagedQuery
    {
        int Offset { get; }

        int Limit { get; }
    }
}
