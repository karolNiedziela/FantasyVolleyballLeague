namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class RosterStaffMember : Entity
    {
        public Guid RosterId { get; private set; }

        public Roster Roster { get; private set; } = default!;

        public Guid StaffMemberId { get; private set; }

        public StaffMember StaffMember { get; private set; } = default!;

        public int RoleId { get; private set; }

        public StaffRole Role { get; private set; } = default!;

        private RosterStaffMember() { }

        public RosterStaffMember(Guid rosterId, Guid staffMemberId, int roleId)
        {
            RosterId = rosterId;
            StaffMemberId = staffMemberId;
            RoleId = roleId;
        }
    }
}
