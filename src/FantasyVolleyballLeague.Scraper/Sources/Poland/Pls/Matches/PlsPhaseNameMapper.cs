using FantasyVolleyballLeague.Shared.Constants;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    internal static class PlsPhaseNameMapper
    {
        private static readonly Dictionary<string, string> Mappings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Faza Zasadnicza"] = PhaseNames.RegularSeason,
            ["Faza Play-Off"] = PhaseNames.Playoff,
            ["Faza Play-Out"] = PhaseNames.Relegation,
        };

        public static string Map(string polishName)
        {
            var trimmed = polishName.Trim();
            return Mappings.TryGetValue(trimmed, out var english) ? english : trimmed;
        }
    }
}
