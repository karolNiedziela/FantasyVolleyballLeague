using FantasyVolleyballLeague.Shared.Constants;

namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class PhaseType
    {
        public static readonly PhaseType RegularSeason = new(1, "regular_season", PhaseNames.RegularSeason);
        public static readonly PhaseType Playoff = new(2, "playoff", PhaseNames.Playoff);
        public static readonly PhaseType Relegation = new(3, "relegation", PhaseNames.Relegation);

        public int Id { get; private set; }

        /// <summary>
        /// The value the API uses for this phase type. Part of the public contract, so it never changes.
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;

        private PhaseType() { }

        private PhaseType(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public static IReadOnlyCollection<PhaseType> All => [RegularSeason, Playoff, Relegation];

        public static PhaseType? FromName(string name)
        {
            return All.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static PhaseType? FromCode(string code)
        {
            return All.FirstOrDefault(t => string.Equals(t.Code, code, StringComparison.Ordinal));
        }
    }
}
