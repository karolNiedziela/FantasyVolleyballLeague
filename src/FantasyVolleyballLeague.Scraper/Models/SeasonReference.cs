using System.Text.Json.Serialization;

namespace FantasyVolleyballLeague.Scraper.Models
{
    /// <param name="ExternalId">The "tour" id used by the league website, e.g. 53 for 2026/27.</param>
    internal sealed record SeasonReference(int StartYear, int EndYear, int? ExternalId)
    {
        /// <summary>
        /// Season folder name in the raw data store, e.g. "2026-2027". Derived, so not written to the JSON files.
        /// </summary>
        [JsonIgnore]
        public string FolderName => $"{StartYear}-{EndYear}";
    }
}
