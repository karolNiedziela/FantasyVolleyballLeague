namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// A player of a roster as the import sees it. <see cref="From"/> and <see cref="To"/> are the dates the
    /// player belonged to the club (<c>null</c> = unknown, so the player is assumed to be there for the whole season).
    /// </summary>
    internal sealed record RosterMember(Guid PlayerId, int? ShirtNumber, string NameKey, DateOnly? From, DateOnly? To)
    {
        /// <summary>
        /// The website's dates are not exact (a stint may start the day after the first match the player played, and
        /// match dates are stored in UTC), so a match one day outside the stint still counts as inside it.
        /// </summary>
        public const int ToleranceDays = 1;

        public bool CoversDate(DateOnly date)
        {
            var afterStart = From is null || date >= From.Value.AddDays(-ToleranceDays);
            var beforeEnd = To is null || date <= To.Value.AddDays(ToleranceDays);

            return afterStart && beforeEnd;
        }
    }
}
