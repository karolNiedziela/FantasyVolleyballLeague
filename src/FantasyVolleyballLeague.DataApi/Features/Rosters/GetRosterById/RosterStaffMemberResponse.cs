namespace FantasyVolleyballLeague.DataApi.Features.Rosters.GetRosterById
{
    internal sealed class RosterStaffMemberResponse
    {
        public Guid StaffMemberId { get; init; }

        public string FullName { get; init; } = string.Empty;

        /// <summary>
        /// Code of the role: head_coach, second_coach, third_coach or president.
        /// </summary>
        public string Role { get; init; } = string.Empty;
    }
}
