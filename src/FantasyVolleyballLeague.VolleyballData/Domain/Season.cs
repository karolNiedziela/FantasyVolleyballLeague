namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class Season : Entity
    {
        public string Name { get; private set; } = string.Empty;

        public int StartYear { get; private set; }

        public int EndYear { get; private set; }

        public int? ExternalId { get; private set; }

        public string LeagueName { get; private set; } = string.Empty;

        public Guid LeagueId { get; private set; }

        public League League { get; private set; } = default!;

        private Season() { }

        public Season(int startYear, int endYear, Guid leagueId, string leagueName, int? externalId)
        {
            Name = $"{startYear}-{endYear}";
            StartYear = startYear;
            EndYear = endYear;
            LeagueId = leagueId;
            LeagueName = leagueName;
            ExternalId = externalId;
        }

        public void AssignExternalId(int? externalId)
        {
            ExternalId = externalId ?? ExternalId;
        }

        public void SetLeagueName(string leagueName)
        {
            LeagueName = leagueName;
        }
    }
}
