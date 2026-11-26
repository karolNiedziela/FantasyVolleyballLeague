using FantasyVolleyballLeague.Shared.Text;

namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    /// <summary>
    /// A club in a given season with its players. <see cref="Club"/> links the rosters of one club across seasons and
    /// leagues.
    /// </summary>
    internal sealed class Roster : Entity, ISearchableName
    {
        private readonly List<RosterPlayer> _players = [];
        private readonly List<RosterStaffMember> _staff = [];

        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// <see cref="Name"/> as <see cref="NameNormalizer.Normalize"/> returns it: the column the name search
        /// (<c>?q=</c>) matches against.
        /// </summary>
        public string NormalizedName { get; private set; } = string.Empty;

        public int? ExternalId { get; private set; }

        public Guid LeagueId { get; private set; }

        public League League { get; private set; } = default!;

        public Guid SeasonId { get; private set; }

        public Season Season { get; private set; } = default!;

        public Guid ClubId { get; private set; }

        public Club Club { get; private set; } = default!;

        public IReadOnlyCollection<RosterPlayer> Players => _players.AsReadOnly();

        public IReadOnlyCollection<RosterStaffMember> Staff => _staff.AsReadOnly();

        private Roster() { }

        public Roster(string name, Guid leagueId, Guid seasonId, Guid clubId, int? externalId)
        {
            Name = name;
            NormalizedName = NameNormalizer.Normalize(name);
            LeagueId = leagueId;
            SeasonId = seasonId;
            ClubId = clubId;
            ExternalId = externalId;
        }

        public void AssignClub(Guid clubId)
        {
            ClubId = clubId;
        }

        public void Rename(string name)
        {
            Name = name;
            NormalizedName = NameNormalizer.Normalize(name);
        }

        /// <summary>
        /// Recomputes <see cref="NormalizedName"/> from <see cref="Name"/>, e.g. for rows that were stored before
        /// the column existed or before the normalization rules changed.
        /// </summary>
        public void RefreshNormalizedName()
        {
            NormalizedName = NameNormalizer.Normalize(Name);
        }

        public void AssignExternalId(int? externalId)
        {
            ExternalId = externalId ?? ExternalId;
        }
    }
}
