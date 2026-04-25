using FantasyVolleyballLeague.Worker.StatisticsScrapper;
using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Models;
using FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga.Parsers;
using HtmlAgilityPack;
using System.Globalization;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga
{
    public sealed class PlusligaMatchStatisticsScrapper : IMatchStatisticsScrapper
    {
        public async Task<MatchRecord?> GetMatchStatisticsAsync(int matchId, PlaywrightSession session)
        {
            var context = session.Context;
            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync($"{UrlConstants.PlusLigaMatchDetailsUrl}{matchId}.html");

                var html = await page.ContentAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var gameDateLocator = page.Locator("div.game-date").First;
                if (await gameDateLocator.CountAsync() == 0)
                {
                    Console.WriteLine($"Match {matchId}: no game-date element found, skipping.");
                    return null;
                }

                var gameDateText = await gameDateLocator.InnerTextAsync();
                if (!TryParseMatchDate(gameDateText, out var matchDate) || matchDate > DateTime.UtcNow)
                    return null;

                MatchTeamStatistics firstTeamStatistics, secondTeamStatistics;
                List<SetScore> sets;
                int firstTeamSetsWon, secondTeamSetsWon;

                if (doc.DocumentNode.SelectSingleNode("//iframe[contains(@class,'widget-team')]") is not null)
                {
                    var teamATask = WidgetParser.GetTeamStatisticsAsync(page, context, "iframe.widget-team-a");
                    var teamBTask = WidgetParser.GetTeamStatisticsAsync(page, context, "iframe.widget-team-b");
                    firstTeamStatistics = await teamATask;
                    secondTeamStatistics = await teamBTask;
                    (firstTeamSetsWon, secondTeamSetsWon) = GetSetsWonByTeam(doc);
                    sets = GetSetScores(doc, firstTeamSetsWon + secondTeamSetsWon);
                }
                else
                {
                    var coloredTables = doc.DocumentNode.SelectNodes("//table[contains(@class,'table-colored')]");
                    if (coloredTables is not null && coloredTables.Count >= 2)
                    {
                        firstTeamStatistics = TableColoredParser.GetTeamStatistics(doc, 0);
                        secondTeamStatistics = TableColoredParser.GetTeamStatistics(doc, 1);
                        (firstTeamSetsWon, secondTeamSetsWon) = GetSetsWonByTeam(doc);
                        sets = GetSetScores(doc, firstTeamSetsWon + secondTeamSetsWon);
                    }
                    else
                    {
                        var earlyTables = doc.DocumentNode.SelectNodes("//table[.//a[contains(@href,'/players/id/')]]")
                            ?.OfType<HtmlNode>()
                            .Where(t => (t.SelectNodes(".//tr")?.Count ?? 0) >= 3)
                            .ToList();
                        var teamNames = doc.DocumentNode.SelectNodes("//h3[@class='notranslate']")
                                       ?? doc.DocumentNode.SelectNodes("//h3");
                        firstTeamStatistics = ArchiveTableParser.GetTeamStatistics(earlyTables?.ElementAtOrDefault(0), teamNames?[0]?.InnerText.Trim());
                        secondTeamStatistics = ArchiveTableParser.GetTeamStatistics(earlyTables?.ElementAtOrDefault(1), teamNames?[1]?.InnerText.Trim());
                        sets = ArchiveTableParser.GetSetScores(doc);
                        firstTeamSetsWon = sets.Count(s => s.TeamAPoints > s.TeamBPoints);
                        secondTeamSetsWon = sets.Count(s => s.TeamBPoints > s.TeamAPoints);
                    }
                }

                firstTeamStatistics.SetsWon = firstTeamSetsWon;
                firstTeamStatistics.Won = firstTeamSetsWon > secondTeamSetsWon;
                secondTeamStatistics.SetsWon = secondTeamSetsWon;
                secondTeamStatistics.Won = secondTeamSetsWon > firstTeamSetsWon;

                var attendanceText = GetTableValue(doc, "Liczba widzów:");
                int? attendance = int.TryParse(attendanceText, out var att) ? att : null;

                return new MatchRecord(
                    matchId,
                    matchDate,
                    GetTableValue(doc, "Numer meczu:"),
                    GetTableValue(doc, "MVP:"),
                    attendance,
                    GetTableValue(doc, "Sędzia pierwszy:"),
                    GetTableValue(doc, "Sędzia drugi:"),
                    GetTableValue(doc, "Komisarz:"),
                    GetTableValue(doc, "Nazwa:"),
                    GetTableValue(doc, "Miasto:"),
                    sets,
                    firstTeamStatistics,
                    secondTeamStatistics);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        private static readonly TimeZoneInfo PolishTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        private static bool TryParseMatchDate(string text, out DateTime matchDate)
        {
            if (!DateTime.TryParseExact(text.Trim(), "dd.MM.yyyy, HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                matchDate = default;
                return false;
            }
            matchDate = TimeZoneInfo.ConvertTimeToUtc(parsed, PolishTimeZone);
            return true;
        }

        private static (int FirstTeamSetsWon, int SecondTeamSetsWon) GetSetsWonByTeam(HtmlDocument doc)
            => (GetSetsWon(doc, "teamAStatus"), GetSetsWon(doc, "teamBStatus"));

        private static int GetSetsWon(HtmlDocument document, string teamStatus)
        {
            var setsWon = document.DocumentNode.Descendants("span")
                .FirstOrDefault(span =>
                {
                    var teamAttribute = span.Attributes
                        .FirstOrDefault(attr => attr.Name.Equals("data-synced-games-class", StringComparison.Ordinal));
                    return teamAttribute != null && teamAttribute.Value.Equals(teamStatus, StringComparison.Ordinal);
                })?.InnerText.Trim() ?? "-1";

            return int.Parse(setsWon);
        }

        private static List<SetScore> GetSetScores(HtmlDocument doc, int totalSets)
        {
            var sets = new List<SetScore>();
            for (var i = 1; i <= totalSets; i++)
            {
                var teamAText = doc.DocumentNode.Descendants("span")
                    .FirstOrDefault(s => s.GetAttributeValue("data-synced-games-content", "") == $"set{i}pointsTeamA")
                    ?.InnerText.Trim();
                var teamBText = doc.DocumentNode.Descendants("span")
                    .FirstOrDefault(s => s.GetAttributeValue("data-synced-games-content", "") == $"set{i}pointsTeamB")
                    ?.InnerText.Trim();

                if (!int.TryParse(teamAText, out var teamAScore) || !int.TryParse(teamBText, out var teamBScore))
                    break;

                sets.Add(new SetScore(teamAScore, teamBScore));
            }
            return sets;
        }

        private static string? GetTableValue(HtmlDocument doc, string thText)
        {
            var th = doc.DocumentNode.Descendants("th")
                .FirstOrDefault(n => n.InnerText.Trim() == thText);

            return th?.ParentNode?.SelectSingleNode("td")?.InnerText.Trim();
        }
    }
}
