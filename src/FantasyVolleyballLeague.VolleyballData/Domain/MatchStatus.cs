namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class MatchStatus
    {
        public static readonly MatchStatus Scheduled = new(1, "scheduled", "Scheduled");
        public static readonly MatchStatus Finished = new(2, "finished", "Finished");

        public int Id { get; private set; }

        /// <summary>
        /// The value the API uses for this status. Part of the public contract, so it never changes.
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;

        private MatchStatus() { }

        private MatchStatus(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public static IReadOnlyCollection<MatchStatus> All => [Scheduled, Finished];

        public static MatchStatus? FromName(string name)
        {
            return All.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static MatchStatus? FromCode(string code)
        {
            return All.FirstOrDefault(s => string.Equals(s.Code, code, StringComparison.Ordinal));
        }
    }
}
