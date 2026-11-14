namespace FantasyVolleyballLeague.Importer.Records
{
    internal sealed record RosterPlayerFile
    {
        public int ExternalId { get; init; }

        /// <summary>
        /// "First Last".
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        public string? Position { get; init; }

        public int? ShirtNumber { get; init; }

        public DateOnly? DateOfBirth { get; init; }

        public int? Height { get; init; }

        public int? Weight { get; init; }

        public int? AttackReach { get; init; }

        /// <summary>
        /// Stints in clubs during the season of the roster. Missing in files scraped before the career was read.
        /// </summary>
        public IReadOnlyList<CareerEntryFile> Career { get; init; } = [];
    }
}
