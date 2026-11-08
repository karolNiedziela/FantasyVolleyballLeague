namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class MatchStatus
    {
        public static readonly MatchStatus Scheduled = new(1, "Scheduled");
        public static readonly MatchStatus Finished = new(2, "Finished");

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        private MatchStatus() { }

        private MatchStatus(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public static IReadOnlyCollection<MatchStatus> All => [Scheduled, Finished];

        public static MatchStatus? FromName(string name)
        {
            return All.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
