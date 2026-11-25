using System.Globalization;
using System.Text;
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

        /// <summary>
        /// Clean text of the node split into lines at <c>br</c>, <c>p</c> and <c>div</c>; comments are skipped.
        /// </summary>
        public static IReadOnlyList<string> TextLines(this HtmlNode node)
        {
            var lines = new List<string>();
            var line = new StringBuilder();
            AppendLines(node, line, lines);
            AddLine(line, lines);
            return lines;
        }

        public static int? ParseIntOrNull(string? text)
        {
            if (text is null)
            {
                return null;
            }

            return int.TryParse(CleanText(text), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
        }

        private static void AppendLines(HtmlNode node, StringBuilder line, List<string> lines)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    line.Append(child.InnerText);
                }
                else if (child.NodeType == HtmlNodeType.Element && child.Name is "br" or "p" or "div")
                {
                    AddLine(line, lines);
                    AppendLines(child, line, lines);
                    AddLine(line, lines);
                }
                else if (child.NodeType == HtmlNodeType.Element)
                {
                    AppendLines(child, line, lines);
                }
            }
        }

        private static void AddLine(StringBuilder line, List<string> lines)
        {
            var text = CleanText(line.ToString());
            if (text.Length > 0)
            {
                lines.Add(text);
            }

            line.Clear();
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();
    }
}
