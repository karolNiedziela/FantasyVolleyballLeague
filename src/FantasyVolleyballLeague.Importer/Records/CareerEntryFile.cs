namespace FantasyVolleyballLeague.Importer.Records
{
    internal sealed record CareerEntryFile
    {
        public int ClubExternalId { get; init; }

        public DateOnly From { get; init; }

        public DateOnly To { get; init; }

        public int? ShirtNumber { get; init; }

        /// <summary>
        /// English position name, e.g. "Outside Hitter".
        /// </summary>
        public string? Position { get; init; }
    }
}
