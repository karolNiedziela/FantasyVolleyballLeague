using FantasyVolleyballLeague.Shared.Text;

namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    /// <summary>
    /// A person, independent of rosters and seasons (season membership is <see cref="RosterPlayer"/>).
    /// <see cref="ExternalId"/> is the player id on the league website; it is missing only for players
    /// the importer could not resolve. Website ids are only unique within one gender (men's and women's
    /// sites number their players independently), so <see cref="ExternalId"/> is unique per <see cref="GenderId"/>.
    /// </summary>
    internal sealed class Player : Entity
    {
        public int GenderId { get; private set; }

        public LeagueGender Gender { get; private set; } = default!;

        public int? ExternalId { get; private set; }

        /// <summary>
        /// "First Last".
        /// </summary>
        public string FullName { get; private set; } = string.Empty;

        /// <summary>
        /// <see cref="FullName"/> as <see cref="NameNormalizer.Normalize"/> returns it: the column the name search
        /// (<c>?q=</c>) matches against.
        /// </summary>
        public string NormalizedName { get; private set; } = string.Empty;

        public DateOnly? DateOfBirth { get; private set; }

        public int? HeightCm { get; private set; }

        public int? WeightKg { get; private set; }

        public int? AttackReachCm { get; private set; }

        private Player() { }

        public Player(int genderId, string fullName, int? externalId)
        {
            GenderId = genderId;
            FullName = fullName;
            NormalizedName = NameNormalizer.Normalize(fullName);
            ExternalId = externalId;
        }

        public void UpdateProfile(string fullName, DateOnly? dateOfBirth, int? heightCm, int? weightKg, int? attackReachCm)
        {
            FullName = fullName;
            NormalizedName = NameNormalizer.Normalize(fullName);
            DateOfBirth = dateOfBirth ?? DateOfBirth;
            HeightCm = heightCm ?? HeightCm;
            WeightKg = weightKg ?? WeightKg;
            AttackReachCm = attackReachCm ?? AttackReachCm;
        }

        /// <summary>
        /// Recomputes <see cref="NormalizedName"/> from <see cref="FullName"/>, e.g. for rows that were stored before
        /// the column existed or before the normalization rules changed.
        /// </summary>
        public void RefreshNormalizedName()
        {
            NormalizedName = NameNormalizer.Normalize(FullName);
        }
    }
}
