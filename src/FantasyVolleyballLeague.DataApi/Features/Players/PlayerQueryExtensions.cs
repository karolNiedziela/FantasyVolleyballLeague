using FantasyVolleyballLeague.DataApi.Common.API.Summaries;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.Features.Players
{
    internal static class PlayerQueryExtensions
    {
        public static IQueryable<Player> WhereGender(this IQueryable<Player> players, int? genderId)
        {
            if (!genderId.HasValue)
            {
                return players;
            }

            return players.Where(x => x.GenderId == genderId.Value);
        }

        public static IQueryable<RosterPlayer> WhereRoster(this IQueryable<RosterPlayer> rosterPlayers, Guid? rosterId)
        {
            if (!rosterId.HasValue)
            {
                return rosterPlayers;
            }

            return rosterPlayers.Where(x => x.RosterId == rosterId.Value);
        }

        public static IQueryable<RosterPlayer> WhereSeason(this IQueryable<RosterPlayer> rosterPlayers, Guid? seasonId)
        {
            if (!seasonId.HasValue)
            {
                return rosterPlayers;
            }

            return rosterPlayers.Where(x => x.Roster.SeasonId == seasonId.Value);
        }

        public static IQueryable<RosterPlayer> WhereLeague(this IQueryable<RosterPlayer> rosterPlayers, Guid? leagueId)
        {
            if (!leagueId.HasValue)
            {
                return rosterPlayers;
            }

            return rosterPlayers.Where(x => x.Roster.LeagueId == leagueId.Value);
        }

        /// <summary>
        /// Orders the roster players of one player so that the current one is first: newest season first; within a
        /// season (a transfer) the latest known <c>MemberFrom</c> first, roster players without the date after the
        /// dated ones. Use it on a query of its own; inside a correlated subquery (<c>WherePosition</c>,
        /// <c>ProjectToResponse</c>) EF does not translate a method call, so those spell the same order out.
        /// </summary>
        public static IOrderedQueryable<RosterPlayer> OrderByMostRecent(this IQueryable<RosterPlayer> rosterPlayers)
        {
            return rosterPlayers
                .OrderByDescending(rosterPlayer => rosterPlayer.Roster.Season.StartYear)
                .ThenBy(rosterPlayer => rosterPlayer.MemberFrom == null)
                .ThenByDescending(rosterPlayer => rosterPlayer.MemberFrom)
                .ThenByDescending(rosterPlayer => rosterPlayer.Id);
        }

        /// <summary>
        /// Keeps the players who are among <paramref name="rosterPlayers"/> (the ones left by <c>WhereRoster</c>,
        /// <c>WhereSeason</c> and <c>WhereLeague</c>). One <c>EXISTS</c>, so a player with several roster players
        /// (a transfer within a season, several seasons) appears once.
        /// </summary>
        public static IQueryable<Player> WhereOnRosters(this IQueryable<Player> players, IQueryable<RosterPlayer> rosterPlayers)
        {
            return players.Where(player => rosterPlayers.Any(rosterPlayer => rosterPlayer.PlayerId == player.Id));
        }

        /// <summary>
        /// Keeps the players whose position is <paramref name="positionCode"/>: the position of their current roster
        /// player (the most recent one among <paramref name="rosterPlayers"/>), the same value
        /// <see cref="PlayerResponse.Position"/> returns.
        /// </summary>
        public static IQueryable<Player> WherePosition(
            this IQueryable<Player> players,
            IQueryable<RosterPlayer> rosterPlayers,
            string? positionCode)
        {
            if (positionCode is null)
            {
                return players;
            }

            return players.Where(player => rosterPlayers
                .Where(rosterPlayer => rosterPlayer.PlayerId == player.Id)
                .OrderByDescending(rosterPlayer => rosterPlayer.Roster.Season.StartYear)
                .ThenBy(rosterPlayer => rosterPlayer.MemberFrom == null)
                .ThenByDescending(rosterPlayer => rosterPlayer.MemberFrom)
                .ThenByDescending(rosterPlayer => rosterPlayer.Id)
                .Select(rosterPlayer => rosterPlayer.Position == null ? null : rosterPlayer.Position.Code)
                .FirstOrDefault() == positionCode);
        }

        public static IQueryable<PlayerResponse> ProjectToResponse(
            this IQueryable<Player> players,
            IQueryable<RosterPlayer> rosterPlayers)
        {
            return players
                .Select(player => new
                {
                    Player = player,
                    Current = rosterPlayers
                        .Where(rosterPlayer => rosterPlayer.PlayerId == player.Id)
                        .OrderByDescending(rosterPlayer => rosterPlayer.Roster.Season.StartYear)
                        .ThenBy(rosterPlayer => rosterPlayer.MemberFrom == null)
                        .ThenByDescending(rosterPlayer => rosterPlayer.MemberFrom)
                        .ThenByDescending(rosterPlayer => rosterPlayer.Id)
                        .FirstOrDefault()
                })
                .Select(x => new PlayerResponse
                {
                    Id = x.Player.Id,
                    FullName = x.Player.FullName,
                    Gender = x.Player.Gender.Code,
                    DateOfBirth = x.Player.DateOfBirth,
                    HeightCm = x.Player.HeightCm,
                    WeightKg = x.Player.WeightKg,
                    AttackReachCm = x.Player.AttackReachCm,
                    Roster = x.Current == null
                        ? null
                        : new RosterSummaryResponse { Id = x.Current.RosterId, Name = x.Current.Roster.Name },
                    Position = x.Current == null || x.Current.Position == null ? null : x.Current.Position.Code
                });
        }
    }
}
