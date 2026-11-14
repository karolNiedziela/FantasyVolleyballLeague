namespace FantasyVolleyballLeague.Importer.Records
{
    internal sealed record CareerEntryFile
    {
        public int ClubExternalId { get; init; }

        public DateOnly From { get; init; }

        public DateOnly To { get; init; }

        public int? ShirtNumber { get; init; }

        public string? Position { get; init; }
    }
}
