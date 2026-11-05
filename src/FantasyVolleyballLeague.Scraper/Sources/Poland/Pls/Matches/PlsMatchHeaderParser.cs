using System.Globalization;
using System.Text.RegularExpressions;
using FantasyVolleyballLeague.Scraper.Models;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    /// <summary>
    /// Parses the match header (<c>div.game-box-big</c>) and the match information tables.
    /// All selectors are scoped to the header box: the site-wide score ticker uses the same
    /// <c>data-synced-games-*</c> attributes for other matches.
    /// </summary>
    internal static partial class PlsMatchHeaderParser
    {
        private const int MaxSets = 5;

        private static readonly TimeZoneInfo PolishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

        public static PlsMatchHeader Parse(HtmlDocument document)
        {
            var box = document.DocumentNode.FindDescendantWithClass("div", "game-box", "game-box-big")
                ?? throw new InvalidOperationException("Match header (div.game-box-big) not found.");

            var status = box.HasClass("complete") ? MatchStatus.Finished : MatchStatus.Scheduled;
            var homeSetsWon = HtmlNodeExtensions.ParseIntOrNull(GetSyncedContent(box, "setsTeamA")?.InnerText);
            var awaySetsWon = HtmlNodeExtensions.ParseIntOrNull(GetSyncedContent(box, "setsTeamB")?.InnerText);
            var (dateUtc, isTimeConfirmed) = ParseDate(box.FindDescendantWithClass("div", "game-date")?.CleanText());
            var mvpCell = GetInfoCell(document, "MVP:");

            return new PlsMatchHeader
            {
                Status = status,
                DateUtc = dateUtc,
                IsTimeConfirmed = isTimeConfirmed,
                HomeRoster = ParseRoster(box, "left", status == MatchStatus.Finished ? homeSetsWon : null),
                AwayRoster = ParseRoster(box, "right", status == MatchStatus.Finished ? awaySetsWon : null),
                Sets = status == MatchStatus.Finished ? ParseSets(box, homeSetsWon, awaySetsWon) : [],
                MatchNumber = GetInfoValue(document, "Numer meczu:"),
                Attendance = HtmlNodeExtensions.ParseIntOrNull(GetInfoValue(document, "Liczba widzów:")),
                Mvp = mvpCell is null ? null : ParseMvp(mvpCell),
                FirstReferee = GetInfoValue(document, "Sędzia pierwszy:"),
                SecondReferee = GetInfoValue(document, "Sędzia drugi:"),
                Commissioner = GetInfoValue(document, "Komisarz:"),
                VenueName = GetInfoValue(document, "Nazwa:"),
                VenueCity = GetInfoValue(document, "Miasto:"),
            };
        }

        /// <summary>
        /// Parses "dd.MM.yyyy, HH:mm" (Polish local time) into UTC. A date without a time
        /// (time not confirmed yet) is returned as local midnight with <c>IsTimeConfirmed = false</c>.
        /// </summary>
        public static (DateTime? DateUtc, bool IsTimeConfirmed) ParseDate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return (null, false);
            }

            var match = DateRegex().Match(text);
            if (!match.Success)
            {
                return (null, false);
            }

            var date = DateTime.ParseExact(match.Groups["date"].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var hasTime = match.Groups["time"].Success;
            if (hasTime)
            {
                date = date.Add(TimeSpan.ParseExact(match.Groups["time"].Value, @"hh\:mm", CultureInfo.InvariantCulture));
            }

            var local = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            return (TimeZoneInfo.ConvertTimeToUtc(local, PolishTimeZone), hasTime);
        }

        private static PlsMatchHeaderRoster ParseRoster(HtmlNode box, string side, int? setsWon)
        {
            var link = box.FindDescendantWithClass("div", "game-team", side)?.Descendants("a").FirstOrDefault();
            var name = link?.CleanText();

            return new PlsMatchHeaderRoster(
                string.IsNullOrEmpty(name) ? "Unknown Team" : name,
                PlsExternalIds.FromTeamUrl(link?.GetAttributeValue("href", string.Empty)),
                setsWon);
        }

        private static List<SetScore> ParseSets(HtmlNode box, int? homeSetsWon, int? awaySetsWon)
        {
            var maxSets = homeSetsWon is not null && awaySetsWon is not null
                ? Math.Min(homeSetsWon.Value + awaySetsWon.Value, MaxSets)
                : MaxSets;

            var sets = new List<SetScore>();
            for (var setNumber = 1; setNumber <= MaxSets && sets.Count < maxSets; setNumber++)
            {
                var home = HtmlNodeExtensions.ParseIntOrNull(GetSyncedContent(box, $"set{setNumber}pointsTeamA")?.InnerText);
                var away = HtmlNodeExtensions.ParseIntOrNull(GetSyncedContent(box, $"set{setNumber}pointsTeamB")?.InnerText);

                if (home is null || away is null || (home == 0 && away == 0))
                {
                    break;
                }

                sets.Add(new SetScore(home.Value, away.Value));
            }

            return sets;
        }

        private static PlayerReference? ParseMvp(HtmlNode cell)
        {
            var name = cell.CleanText();
            if (name.Length == 0)
            {
                return null;
            }

            var link = cell.Descendants("a").FirstOrDefault();
            return new PlayerReference(name, PlsExternalIds.FromPlayerUrl(link?.GetAttributeValue("href", string.Empty)));
        }

        private static HtmlNode? GetSyncedContent(HtmlNode box, string content)
        {
            return box.Descendants("span")
                .FirstOrDefault(span => span.GetAttributeValue("data-synced-games-content", "") == content);
        }

        private static string? GetInfoValue(HtmlDocument document, string label)
        {
            var value = GetInfoCell(document, label)?.CleanText();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static HtmlNode? GetInfoCell(HtmlDocument document, string label)
        {
            var th = document.DocumentNode.Descendants("th")
                .FirstOrDefault(n => n.CleanText() == label);

            return th?.ParentNode?.Element("td");
        }

        [GeneratedRegex(@"(?<date>\d{2}\.\d{2}\.\d{4})(?:\s*,\s*(?<time>\d{2}:\d{2}))?")]
        private static partial Regex DateRegex();
    }
}
