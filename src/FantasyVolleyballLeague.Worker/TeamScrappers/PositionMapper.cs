namespace FantasyVolleyballLeague.Worker.TeamScrappers
{
    public static class PositionMapper
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
            => PolishToEnglish.TryGetValue(polishPosition, out var english) ? english : polishPosition;
    }
}
