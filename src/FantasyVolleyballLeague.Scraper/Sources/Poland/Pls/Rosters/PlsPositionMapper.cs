using FantasyVolleyballLeague.Contracts;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters
{
    internal static class PlsPositionMapper
    {
        private static readonly Dictionary<string, string> PolishToEnglish = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Przyjmujący"] = PositionNames.OutsideHitter,
            ["Przyjmująca"] = PositionNames.OutsideHitter,
            ["Atakujący"] = PositionNames.OppositeHitter,
            ["Atakująca"] = PositionNames.OppositeHitter,
            ["Środkowy"] = PositionNames.MiddleBlocker,
            ["Środkowa"] = PositionNames.MiddleBlocker,
            ["Rozgrywający"] = PositionNames.Setter,
            ["Rozgrywająca"] = PositionNames.Setter,
            ["Libero"] = PositionNames.Libero,
        };

        public static string Map(string polishPosition)
        {
            var trimmed = polishPosition.Trim();
            return PolishToEnglish.TryGetValue(trimmed, out var english) ? english : trimmed;
        }
    }
}
