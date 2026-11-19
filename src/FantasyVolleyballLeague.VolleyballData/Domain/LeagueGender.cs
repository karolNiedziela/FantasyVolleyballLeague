namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class LeagueGender
    {
        public static readonly LeagueGender Men = new(1, "men", "Men");
        public static readonly LeagueGender Women = new(2, "women", "Women");

        public int Id { get; private set; }

        /// <summary>
        /// The value the API uses for this gender. Part of the public contract, so it never changes.
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;

        private LeagueGender() { }

        private LeagueGender(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public static IReadOnlyCollection<LeagueGender> All => [Men, Women];

        public static LeagueGender? FromId(int id)
        {
            return All.FirstOrDefault(g => g.Id == id);
        }

        public static LeagueGender? FromCode(string code)
        {
            return All.FirstOrDefault(g => string.Equals(g.Code, code, StringComparison.Ordinal));
        }
    }
}
