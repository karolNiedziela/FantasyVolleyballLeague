namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class LeagueGender
    {
        public static readonly LeagueGender Men = new(1, "Men");
        public static readonly LeagueGender Women = new(2, "Women");

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        private LeagueGender() { }

        private LeagueGender(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public static IReadOnlyCollection<LeagueGender> All => [Men, Women];

        public static LeagueGender? FromId(int id)
        {
            return All.FirstOrDefault(g => g.Id == id);
        }
    }
}
