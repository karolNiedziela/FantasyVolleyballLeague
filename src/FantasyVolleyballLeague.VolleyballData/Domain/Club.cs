using FantasyVolleyballLeague.Shared.Text;

namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    /// <summary>
    /// A club across seasons and leagues: it groups the <see cref="Roster"/>s of one club
    /// </summary>
    internal sealed class Club : Entity, ISearchableName
    {
        /// <summary>
        /// The name of the newest roster of the club.
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// <see cref="Name"/> as <see cref="NameNormalizer.Normalize"/> returns it: the column the name search
        /// (<c>?q=</c>) matches against.
        /// </summary>
        public string NormalizedName { get; private set; } = string.Empty;

        public int GenderId { get; private set; }

        public LeagueGender Gender { get; private set; } = default!;

        private Club() { }

        public Club(string name, int genderId)
        {
            Name = name;
            NormalizedName = NameNormalizer.Normalize(name);
            GenderId = genderId;
        }

        public void Rename(string name)
        {
            Name = name;
            NormalizedName = NameNormalizer.Normalize(name);
        }

        public void RefreshNormalizedName()
        {
            NormalizedName = NameNormalizer.Normalize(Name);
        }
    }
}
