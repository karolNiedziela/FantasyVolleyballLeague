using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper
{
    internal static partial class HtmlNodeExtensions
    {
        public static bool HasClass(this HtmlNode node, params string[] classNames)
        {
            var classes = node.GetAttributeValue("class", "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return classNames.All(classes.Contains);
        }

        public static HtmlNode? FindDescendantWithClass(this HtmlNode node, string tag, params string[] classNames)
        {
            return node.Descendants(tag).FirstOrDefault(n => n.HasClass(classNames));
        }

        public static IEnumerable<HtmlNode> FindDescendantsWithClass(this HtmlNode node, string tag, params string[] classNames)
        {
            return node.Descendants(tag).Where(n => n.HasClass(classNames));
        }

        /// <summary>
        /// Inner text with HTML entities (e.g. <c>&amp;nbsp;</c>) decoded and whitespace collapsed.
        /// </summary>
        public static string CleanText(this HtmlNode node)
        {
            return CleanText(node.InnerText);
        }

        public static string CleanText(string text)
        {
            var decoded = HtmlEntity.DeEntitize(text) ?? string.Empty;
            return WhitespaceRegex().Replace(decoded, " ").Trim();
        }

        public static int? ParseIntOrNull(string? text)
        {
            if (text is null)
            {
                return null;
            }

            return int.TryParse(CleanText(text), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();
    }
}
