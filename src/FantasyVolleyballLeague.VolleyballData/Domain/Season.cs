namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class Season : Entity
    {
        public string Name { get; private set; } = string.Empty;

        public int StartYear { get; private set; }

        public int EndYear { get; private set; }

        public int? ExternalId { get; private set; }

        public Guid LeagueId { get; private set; }

        public League League { get; private set; } = default!;

        private Season() { }

        public Season(int startYear, int endYear, Guid leagueId, int? externalId)
        {
            Name = $"{startYear}-{endYear}";
            StartYear = startYear;
            EndYear = endYear;
            LeagueId = leagueId;
            ExternalId = externalId;
        }

        public void AssignExternalId(int? externalId)
        {
            ExternalId = externalId ?? ExternalId;
        }
    }
}
