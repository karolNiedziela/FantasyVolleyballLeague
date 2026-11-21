using System.Linq.Expressions;
using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using FantasyVolleyballLeague.VolleyballData.Domain;
using Microsoft.EntityFrameworkCore;

namespace FantasyVolleyballLeague.DataApi.Features.Seasons.GetSeasonById
{
    internal sealed class GetSeasonByIdQueryHandler : IQueryHandler<GetSeasonByIdQuery, GetSeasonByIdResponse?>
    {
        private readonly IDbContext _dbContext;

        public GetSeasonByIdQueryHandler(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetSeasonByIdResponse?> HandleAsync(GetSeasonByIdQuery query, CancellationToken cancellationToken)
        {
            var allSeasons = _dbContext.Set<Season>();
            Expression<Func<Season, SeasonSummaryResponse>> toSummary = season => new SeasonSummaryResponse
            {
                Id = season.Id,
                Name = season.Name,
                StartYear = season.StartYear,
                EndYear = season.EndYear
            };

            var season = await allSeasons
                .Where(x => x.Id == query.Id)
                .Select(x => new
                {
                    x.Id,
                    x.ExternalId,
                    x.Name,
                    x.StartYear,
                    x.EndYear,
                    IsCurrent = x.StartYear == allSeasons
                        .Where(other => other.LeagueId == x.LeagueId)
                        .Max(other => other.StartYear),
                    League = new LeagueSummaryResponse
                    {
                        Id = x.League.Id,
                        Name = x.League.Name,
                        DivisionNumber = x.League.DivisionNumber,
                        Country = x.League.Country.Code,
                        Gender = x.League.Gender.Code
                    },
                    PreviousSeason = allSeasons
                        .Where(other => other.LeagueId == x.LeagueId && other.StartYear < x.StartYear)
                        .OrderByDescending(other => other.StartYear)
                        .Select(toSummary)
                        .FirstOrDefault(),
                    NextSeason = allSeasons
                        .Where(other => other.LeagueId == x.LeagueId && other.StartYear > x.StartYear)
                        .OrderBy(other => other.StartYear)
                        .Select(toSummary)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (season is null)
            {
                return null;
            }

            var phases = await LoadPhasesAsync(query.Id, cancellationToken);

            return new GetSeasonByIdResponse
            {
                Id = season.Id,
                ExternalId = season.ExternalId,
                Name = season.Name,
                StartYear = season.StartYear,
                EndYear = season.EndYear,
                IsCurrent = season.IsCurrent,
                League = season.League,
                PreviousSeason = season.PreviousSeason,
                NextSeason = season.NextSeason,
                Phases = phases
            };
        }

        private async Task<List<SeasonPhaseResponse>> LoadPhasesAsync(Guid seasonId, CancellationToken cancellationToken)
        {
            var phases = await _dbContext.Set<Phase>()
                .Where(x => x.SeasonId == seasonId)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Id)
                .Select(x => new { x.Id, x.Name, Type = x.PhaseType.Code, x.Order })
                .ToListAsync(cancellationToken);

            var rounds = await _dbContext.Set<Round>()
                .Where(x => x.Phase.SeasonId == seasonId)
                .OrderBy(x => x.StageOrder)
                .ThenBy(x => x.Order)
                .ThenBy(x => x.Id)
                .Select(x => new { x.Id, x.PhaseId, x.Name, x.Order, StageName = x.Stage, x.StageOrder })
                .ToListAsync(cancellationToken);

            var roundsByPhase = rounds.ToLookup(x => x.PhaseId);

            return phases
                .Select(phase => new SeasonPhaseResponse
                {
                    Id = phase.Id,
                    Name = phase.Name,
                    Type = phase.Type,
                    Order = phase.Order,
                    Rounds = roundsByPhase[phase.Id]
                        .Select(round => new SeasonRoundResponse
                        {
                            Id = round.Id,
                            Name = round.Name,
                            Order = round.Order,
                            Stage = string.IsNullOrEmpty(round.StageName)
                                ? null
                                : new StageSummaryResponse { Name = round.StageName, Order = round.StageOrder }
                        })
                        .ToList()
                })
                .ToList();
        }
    }
}
