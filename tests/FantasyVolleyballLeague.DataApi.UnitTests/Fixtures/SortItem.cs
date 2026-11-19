using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Fixtures
{
    internal sealed class SortItem : Entity
    {
        public SortItem(string name, int rank = 1)
        {
            Name = name;
            Rank = rank;
        }

        public string Name { get; }

        public int Rank { get; }
    }

    internal sealed record SortItemQuery(string? Sort) : ISortableQuery<SortItem>
    {
        public static SortMap<SortItem> SortMap { get; } = new SortMap<SortItem>()
            .Add("name", item => item.Name)
            .Add("rank", item => item.Rank)
            .WithDefault("name");
    }
}
