namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters
{
    internal static class PlsPositionMapper
    {
        private static readonly Dictionary<string, string> PolishToEnglish = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Przyjmujący"] = "Outside Hitter",
            ["Przyjmująca"] = "Outside Hitter",
            ["Atakujący"] = "Opposite Hitter",
            ["Atakująca"] = "Opposite Hitter",
            ["Środkowy"] = "Middle Blocker",
            ["Środkowa"] = "Middle Blocker",
            ["Rozgrywający"] = "Setter",
            ["Rozgrywająca"] = "Setter",
            ["Libero"] = "Libero",
        };

        public static string Map(string polishPosition)
        {
            var trimmed = polishPosition.Trim();
            return PolishToEnglish.TryGetValue(trimmed, out var english) ? english : trimmed;
        }
    }
}
