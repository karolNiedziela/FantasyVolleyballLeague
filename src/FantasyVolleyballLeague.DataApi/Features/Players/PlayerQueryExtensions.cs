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
        /// Keeps the players who are among <paramref name="rosterPlayers"/> (the ones left by <c>WhereRoster</c>,
        /// <c>WhereSeason</c> and <c>WhereLeague</c>). One <c>EXISTS</c>, so a player with several roster players
        /// (a transfer within a season, several seasons) appears once.
        /// </summary>
        public static IQueryable<Player> WhereOnRosters(this IQueryable<Player> players, IQueryable<RosterPlayer> rosterPlayers)
        {
            return players.Where(player => rosterPlayers.Any(rosterPlayer => rosterPlayer.PlayerId == player.Id));
        }

        /// <summary>
        /// Keeps the players whose position is <paramref name="positionCode"/>: the position of their most recent
        /// roster player that has one, the same value <see cref="PlayerResponse.Position"/> returns.
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
                .Where(rosterPlayer => rosterPlayer.PlayerId == player.Id && rosterPlayer.PositionId != null)
                .OrderByDescending(rosterPlayer => rosterPlayer.Roster.Season.StartYear)
                .ThenBy(rosterPlayer => rosterPlayer.MemberFrom == null)
                .ThenByDescending(rosterPlayer => rosterPlayer.MemberFrom)
                .ThenByDescending(rosterPlayer => rosterPlayer.Id)
                .Select(rosterPlayer => rosterPlayer.Position == null ? null : rosterPlayer.Position.Code)
                .FirstOrDefault() == positionCode);
        }

        // A player's position is defined twice, here and in WherePosition: the code of the position of the most recent
        // roster player that has one. Newest season first; within a season (a transfer) the latest known MemberFrom
        // first, roster players without the date after the dated ones. It cannot live in one shared helper: EF does
        // not translate a method called inside the expression, and Expression.Compile().Invoke() fails to translate
        // in Where (in Select it was not checked for what SQL it produces). Keep the two copies identical. The tests
        // cover both.
        public static IQueryable<PlayerResponse> ProjectToResponse(
            this IQueryable<Player> players,
            IQueryable<RosterPlayer> rosterPlayers)
        {
            return players.Select(player => new PlayerResponse
            {
                Id = player.Id,
                FullName = player.FullName,
                Gender = player.Gender.Code,
                DateOfBirth = player.DateOfBirth,
                HeightCm = player.HeightCm,
                WeightKg = player.WeightKg,
                AttackReachCm = player.AttackReachCm,
                Position = rosterPlayers
                    .Where(rosterPlayer => rosterPlayer.PlayerId == player.Id && rosterPlayer.PositionId != null)
                    .OrderByDescending(rosterPlayer => rosterPlayer.Roster.Season.StartYear)
                    .ThenBy(rosterPlayer => rosterPlayer.MemberFrom == null)
                    .ThenByDescending(rosterPlayer => rosterPlayer.MemberFrom)
                    .ThenByDescending(rosterPlayer => rosterPlayer.Id)
                    .Select(rosterPlayer => rosterPlayer.Position == null ? null : rosterPlayer.Position.Code)
                    .FirstOrDefault()
            });
        }
    }
}
