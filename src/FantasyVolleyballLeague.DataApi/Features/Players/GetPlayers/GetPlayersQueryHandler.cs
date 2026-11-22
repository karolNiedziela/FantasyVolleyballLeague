using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Search;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetPlayers
{
    internal sealed class GetPlayersQueryHandler : IQueryHandler<GetPlayersQuery, PagedResponse<PlayerResponse>>
    {
        private readonly IDbContext _dbContext;

        public GetPlayersQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<PlayerResponse>> HandleAsync(GetPlayersQuery query, CancellationToken cancellationToken)
        {
            var genderId = query.Gender is null ? null : LeagueGender.FromCode(query.Gender)?.Id;
            var rosterPlayers = _dbContext.Set<RosterPlayer>()
                .WhereRoster(query.RosterId)
                .WhereSeason(query.SeasonId)
                .WhereLeague(query.LeagueId);

            var players = _dbContext.Set<Player>()
                .WhereGender(genderId)
                .WhereNameMatches(query.Search);

            if (query.RosterId.HasValue || query.SeasonId.HasValue || query.LeagueId.HasValue)
            {
                players = players.WhereOnRosters(rosterPlayers);
            }

            return await players
                .WherePosition(rosterPlayers, query.Position)
                .ApplySort(query)
                .ProjectToResponse(rosterPlayers)
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
