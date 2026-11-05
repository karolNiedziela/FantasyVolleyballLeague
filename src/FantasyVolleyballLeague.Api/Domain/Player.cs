namespace FantasyVolleyballLeague.Api.Domain
{
    /// <summary>
    /// A person, independent of rosters and seasons (season membership is <see cref="RosterPlayer"/>).
    /// <see cref="ExternalId"/> is the player id on the league website; it is missing only for players
    /// the importer could not resolve.
    /// </summary>
    internal sealed class Player : Entity
    {
        public int? ExternalId { get; private set; }

        /// <summary>
        /// "First Last".
        /// </summary>
        public string FullName { get; private set; } = string.Empty;

        public DateOnly? DateOfBirth { get; private set; }

        public int? HeightCm { get; private set; }

        public int? WeightKg { get; private set; }

        public int? AttackReachCm { get; private set; }

        private Player() { }

        public Player(string fullName, int? externalId)
        {
            FullName = fullName;
            ExternalId = externalId;
        }

        public void UpdateProfile(string fullName, DateOnly? dateOfBirth, int? heightCm, int? weightKg, int? attackReachCm)
        {
            FullName = fullName;
            DateOfBirth = dateOfBirth ?? DateOfBirth;
            HeightCm = heightCm ?? HeightCm;
            WeightKg = weightKg ?? WeightKg;
            AttackReachCm = attackReachCm ?? AttackReachCm;
        }
    }
}
