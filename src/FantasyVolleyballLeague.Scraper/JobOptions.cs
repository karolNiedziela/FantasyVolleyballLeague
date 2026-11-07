using FantasyVolleyballLeague.Scraper.Models;

namespace FantasyVolleyballLeague.Scraper
{
    internal sealed class JobOptions
    {
        public const string SectionName = "Job";

        public static readonly Dictionary<string, string> SwitchMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["--job"] = $"{SectionName}:{nameof(Kind)}",
            ["--league"] = $"{SectionName}:{nameof(League)}",
            ["--from"] = $"{SectionName}:{nameof(FromSeason)}",
            ["--to"] = $"{SectionName}:{nameof(ToSeason)}",
            ["--current"] = $"{SectionName}:{nameof(CurrentSeasonOnly)}",
            ["--force"] = $"{SectionName}:{nameof(Force)}",
            ["--max-attempts"] = $"{SectionName}:{nameof(MaxMatchAttempts)}",
        };

        public JobKind Kind { get; set; } = JobKind.Matches;

        /// <summary>
        /// League key to process. All configured leagues when empty.
        /// </summary>
        public string? League { get; set; }

        /// <summary>
        /// First season (start year, inclusive) to process.
        /// </summary>
        public int? FromSeason { get; set; }

        /// <summary>
        /// Last season (start year, inclusive) to process.
        /// </summary>
        public int? ToSeason { get; set; }

        public bool CurrentSeasonOnly { get; set; }

        /// <summary>
        /// Re-scrape data that is already stored (complete seasons, finished matches, existing rosters).
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        /// Failed scrape attempts (runs) after which a match is saved without player statistics.
        /// Website blocks are not counted. <see cref="Force"/> starts the count from zero.
        /// </summary>
        public int MaxMatchAttempts { get; set; } = 3;

        public bool IncludesLeague(string leagueKey)
        {
            return string.IsNullOrWhiteSpace(League) || string.Equals(League, leagueKey, StringComparison.OrdinalIgnoreCase);
        }

        public bool IncludesSeason(int startYear)
        {
            return (FromSeason is null || startYear >= FromSeason) && (ToSeason is null || startYear <= ToSeason);
        }

        /// <summary>
        /// Seasons to process, oldest first. With <see cref="CurrentSeasonOnly"/> only the newest listed season.
        /// </summary>
        public IReadOnlyList<Season> SelectSeasons(IReadOnlyList<Season> seasons)
        {
            if (seasons.Count == 0)
            {
                return [];
            }

            var latestStartYear = seasons.Max(s => s.StartYear);

            return [.. seasons
                .Where(s => CurrentSeasonOnly ? s.StartYear == latestStartYear : IncludesSeason(s.StartYear))
                .OrderBy(s => s.StartYear)];
        }
    }
}
