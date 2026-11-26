namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// What to change in the stored staff of a roster so that it matches the roster file, which is the source of truth.
    /// </summary>
    internal sealed record RosterStaffChanges(IReadOnlyList<RosterStaffAssignment> Added, IReadOnlyList<RosterStaffAssignment> Removed)
    {
        public static RosterStaffChanges Between(IReadOnlyCollection<RosterStaffAssignment> stored, IReadOnlyCollection<RosterStaffAssignment> scraped)
        {
            var storedAssignments = stored.ToHashSet();
            var scrapedAssignments = scraped.ToHashSet();

            return new RosterStaffChanges(
                [.. scrapedAssignments.Where(assignment => !storedAssignments.Contains(assignment))],
                [.. storedAssignments.Where(assignment => !scrapedAssignments.Contains(assignment))]);
        }
    }
}
