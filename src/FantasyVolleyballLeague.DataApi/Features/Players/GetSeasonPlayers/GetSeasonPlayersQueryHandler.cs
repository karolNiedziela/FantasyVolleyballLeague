using FantasyVolleyballLeague.DataApi.Common.API.Paging;
using FantasyVolleyballLeague.DataApi.Common.API.Search;
using FantasyVolleyballLeague.DataApi.Common.API.Sorting;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Players.GetSeasonPlayers
{
    internal sealed class GetSeasonPlayersQueryHandler : IQueryHandler<GetSeasonPlayersQuery, PagedResponse<PlayerResponse>?>
    {
        private readonly IDbContext _dbContext;

        public GetSeasonPlayersQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<PlayerResponse>?> HandleAsync(GetSeasonPlayersQuery query, CancellationToken cancellationToken)
        {
            var seasonExists = await _dbContext.Set<Season>()
                .AnyAsync(x => x.Id == query.SeasonId, cancellationToken);

            if (!seasonExists)
            {
                return null;
            }

            var rosterPlayers = _dbContext.Set<RosterPlayer>()
                .WhereRoster(query.RosterId)
                .WhereSeason(query.SeasonId);

            return await _dbContext.Set<Player>()
                .WhereOnRosters(rosterPlayers)
                .WhereNameMatches(query.Search)
                .WherePosition(rosterPlayers, query.Position)
                .ApplySort(query)
                .ProjectToResponse(rosterPlayers)
                .ToPagedResponseAsync(query.Offset, query.Limit, cancellationToken);
        }
    }
}
