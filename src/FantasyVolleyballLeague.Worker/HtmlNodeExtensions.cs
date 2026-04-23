using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Worker
{
    internal static class HtmlNodeExtensions
    {
        public static bool HasClass(this HtmlNode node, params string[] classNames)
        {
            var classes = node.GetAttributeValue("class", "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return classNames.All(classes.Contains);
        }

        public static HtmlNode? FindDescendantWithClass(this HtmlNode node, string tag, params string[] classNames)
            => node.Descendants(tag).FirstOrDefault(n => n.HasClass(classNames));

        public static IEnumerable<HtmlNode> FindDescendantsWithClass(this HtmlNode node, string tag, params string[] classNames)
            => node.Descendants(tag).Where(n => n.HasClass(classNames));
    }
}
