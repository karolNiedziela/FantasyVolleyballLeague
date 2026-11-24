namespace FantasyVolleyballLeague.Importer.Import
{
    /// <summary>
    /// A manual data file after reading and validation. <see cref="Entries"/> is <c>null</c> when the file cannot be
    /// read or has errors, so the caller leaves the data as it was.
    /// </summary>
    internal sealed record ManualDataResult<T>(IReadOnlyList<T>? Entries, IReadOnlyList<string> Errors);
}
