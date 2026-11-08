namespace FantasyVolleyballLeague.Api.Domain
{
    /// <summary>
    /// The only place entity ids are created. Ids are time-ordered UUID v7, so they sort by creation time
    /// (also in PostgreSQL, which compares uuids byte by byte). <c>Guid.NewGuid()</c> is banned (BannedSymbols.txt).
    /// </summary>
    internal static class EntityId
    {
        public static Guid New()
        {
            return Guid.CreateVersion7();
        }
    }
}
