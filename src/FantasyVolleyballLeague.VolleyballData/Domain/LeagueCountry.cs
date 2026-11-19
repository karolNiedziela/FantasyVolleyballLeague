namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class LeagueCountry
    {
        public static readonly LeagueCountry Poland = new(1, "pl", "Poland");

        public int Id { get; private set; }

        /// <summary>
        /// ISO 3166-1 alpha-2 code in lowercase, the value the API uses for this country. Never changes.
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;

        private LeagueCountry() { }

        private LeagueCountry(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public static LeagueCountry? FromId(int id) => All.FirstOrDefault(c => c.Id == id);

        public static LeagueCountry? FromCode(string code)
        {
            return All.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.Ordinal));
        }

        public static IReadOnlyCollection<LeagueCountry> All => [Poland];
    }
}
