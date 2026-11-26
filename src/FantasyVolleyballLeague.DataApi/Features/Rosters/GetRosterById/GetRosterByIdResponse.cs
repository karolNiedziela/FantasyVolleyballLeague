using FantasyVolleyballLeague.DataApi.Common.API.Summaries;

namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosterById
{
    internal sealed class GetRosterByIdResponse
    {
        public Guid Id { get; init; }

        /// <summary>
        /// Id of the club on the source website. Informational: it is not unique across sources and not a filter.
        /// <c>null</c> when the source has none.
        /// </summary>
        public int? ExternalId { get; init; }

        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The club across seasons and leagues: every roster of the club has the same club id. Named after its newest
        /// roster.
        /// </summary>
        public ClubSummaryResponse Club { get; init; } = new();

        public LeagueSummaryResponse League { get; init; } = new();

        public SeasonSummaryResponse Season { get; init; } = new();

        /// <summary>
        /// By shirt number (players without one last), then by name.
        /// </summary>
        public IReadOnlyList<RosterPlayerResponse> Players { get; init; } = [];

        /// <summary>
        /// By role (head, second and third coach, then the president), then by name; a person with two roles is listed
        /// once per role. In the Polish leagues the coaches are known from 2023/24 onwards.
        /// </summary>
        public IReadOnlyList<RosterStaffMemberResponse> Staff { get; init; } = [];
    }
}
