namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga
{
    public static class PhaseNameMapper
    {
        private static readonly Dictionary<string, string> Mappings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Faza Zasadnicza"] = "Regular Season",
            ["Faza Play-Off"] = "Playoff",
            ["Faza Play-Out"] = "Relegation",
        };

        public static string Map(string polishName)
        {
            var trimmed = polishName.Trim();
            return Mappings.TryGetValue(trimmed, out var english) ? english : trimmed;
        }
    }
}
