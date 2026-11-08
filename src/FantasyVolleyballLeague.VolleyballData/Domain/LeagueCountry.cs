namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class LeagueCountry
    {
        public static readonly LeagueCountry Poland = new(1, "Poland");

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        private LeagueCountry() { }

        private LeagueCountry(int id, string name)
        { 
            Id = id; 
            Name = name; 
        }

        public static LeagueCountry? FromId(int id) => All.FirstOrDefault(c => c.Id == id);

        public static IReadOnlyCollection<LeagueCountry> All => [Poland];
    }
}
