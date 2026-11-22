using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Players
{
    internal static class PlayerSortMaps
    {
        public static SortMap<Player> Default => new SortMap<Player>()
            .Add(PlayerSortFields.FullName, x => x.FullName)
            .WithDefault(PlayerSortFields.FullName);
    }
}
