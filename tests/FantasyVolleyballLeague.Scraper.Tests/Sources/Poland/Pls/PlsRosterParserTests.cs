using FantasyVolleyballLeague.Shared.Constants;
using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;
using HtmlAgilityPack;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsRosterParserTests
    {
        [Fact]
        public void ParseClubsReturnsSeasonClubsWithIds()
        {
            var clubs = PlsRosterParser.ParseClubs(Fixture.Load("teams_tour53.html"));

            clubs.Count.ShouldBe(14);
            clubs.ShouldContain(new ClubLink(2, "Aluron CMC Warta Zawiercie"));
            clubs.ShouldContain(new ClubLink(3, "Asseco Resovia Rzeszów"));
        }

        [Fact]
        public void ParseTeamPageReadsRosterEntries()
        {
            var teamPage = PlsRosterParser.ParseTeamPage(Fixture.Load("team_18_tour53.html"));

            teamPage.Name.ShouldBe("GKS Katowice");
            var krulicki = teamPage.Players.Where(player => player.ExternalId == 278).ShouldHaveSingleItem();
            (krulicki.FullName, krulicki.ShirtNumber, krulicki.Position).ShouldBe(("Bartłomiej Krulicki", 1, PositionNames.MiddleBlocker));
            var szymanski = teamPage.Players.Where(player => player.ExternalId == 837).ShouldHaveSingleItem();
            (szymanski.FullName, szymanski.ShirtNumber, szymanski.Position).ShouldBe(("Jakub Szymański", 2, PositionNames.OutsideHitter));
        }

        [Fact]
        public void ParsePlayerProfileReadsValuesByLabel()
        {
            var details = PlsRosterParser.ParsePlayerProfile(Fixture.Load("player_3169_tour53.html"));

            details.ShouldBe(new PlsPlayerDetails(new DateOnly(2000, 1, 18), PositionNames.Setter, 195, 91, 319, 8));
        }

        [Fact]
        public void ParseCareerReadsEveryStintWithClubAndSeason()
        {
            var career = PlsRosterParser.ParseCareer(Fixture.Load("player_1668_tour32.html"));

            career.Count.ShouldBe(9);
            const string opposite = PositionNames.OppositeHitter;
            career.ShouldContain(new PlsCareerRow(83, 32, new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30), 10, opposite));
            career.ShouldContain(new PlsCareerRow(15, 32, new DateOnly(2020, 1, 30), new DateOnly(2020, 3, 25), 94, opposite));
            career.ShouldContain(new PlsCareerRow(68, 35, new DateOnly(2020, 9, 11), new DateOnly(2021, 5, 9), 94, opposite));
        }

        [Fact]
        public void ParseCareerWithoutCareerTableReturnsNothing()
        {
            PlsRosterParser.ParseCareer(Fixture.Load("teams_tour53.html")).ShouldBeEmpty();
        }

        [Fact]
        public void ParseTeamPage_Should_ReadTheCoachesAndThePresident()
        {
            var teamPage = PlsRosterParser.ParseTeamPage(Fixture.Load("team_2_tour52.html"));

            teamPage.Staff.ShouldBe(
            [
                Staff("Michał Winiarski", StaffRoleNames.HeadCoach),
                Staff("Paweł Rusek", StaffRoleNames.SecondCoach),
                Staff("Dominik Posmyk", StaffRoleNames.ThirdCoach),
                Staff("Kryspin Baran", StaffRoleNames.President),
            ]);
        }

        [Theory]
        [InlineData("team_18_tour53.html", "Piotr Karlik", "Jakub Nalepka")]
        [InlineData("team_7_tour50.html", "Krzysztof Michalski", "Dariusz Król")]
        public void ParseTeamPage_Should_SplitTheCoachesOfOneRole(string fixture, string first, string second)
        {
            var teamPage = PlsRosterParser.ParseTeamPage(Fixture.Load(fixture));

            var thirdCoaches = teamPage.Staff.Where(member => member.Role == StaffRoleNames.ThirdCoach);
            thirdCoaches.Select(member => member.FullName).ShouldBe([first, second]);
        }

        [Fact]
        public void ParseTeamPage_Should_ReadNoCoaches_When_ThePageHasNoStaffBox()
        {
            var teamPage = PlsRosterParser.ParseTeamPage(Fixture.Load("team_3_tour17.html"));

            teamPage.Staff.Select(member => member.Role).ShouldBe([StaffRoleNames.President]);
        }

        [Fact]
        public void ParseTeamPage_Should_SkipUnknownRolesInTheStaffBox()
        {
            const string html = """
                <div class="info-box"><h4 class="title">SZTAB</h4>
                <div><b>Pierwszy trener:</b> Jan Kowalski<br /><b>Statystyk:</b> Anna Nowak</div></div>
                """;

            var teamPage = PlsRosterParser.ParseTeamPage(Html(html));

            teamPage.Staff.ShouldBe([Staff("Jan Kowalski", StaffRoleNames.HeadCoach)]);
        }

        [Theory]
        [InlineData("team_7_tour50.html", "Maciej Kochański")]
        [InlineData("team_3_tour17.html", "Piotr Maciąg")]
        [InlineData("team_18_tour53.html", "Sławomir Witek")]
        public void ParseTeamPage_Should_ReadThePresident(string fixture, string president)
        {
            var teamPage = PlsRosterParser.ParseTeamPage(Fixture.Load(fixture));

            var presidents = teamPage.Staff.Where(member => member.Role == StaffRoleNames.President);
            presidents.ShouldHaveSingleItem().FullName.ShouldBe(president);
        }

        [Theory]
        [InlineData("<div><strong>Prezes:</strong>&nbsp;Jan Kowalski</div><div><strong>Dyrektor:</strong> Anna Nowak</div>", "Jan Kowalski")]
        [InlineData("<div class=\"v1info-box\"><div><strong>Prezes klubu</strong>: Jan Kowalski&nbsp;</div></div>", "Jan Kowalski")]
        [InlineData("<p><strong>Prezes klubu:&nbsp;</strong>Jan Kr&oacute;l</p>", "Jan Król")]
        [InlineData("<p><strong>Wiceprezes Zarządu:</strong> Anna Nowak<br /><br /><strong>Prezes Zarządu:</strong> Jan Kowalski</p>", "Jan Kowalski")]
        public void ParseTeamPage_Should_ReadThePresidentFromTheManagementBox(string box, string president)
        {
            var teamPage = PlsRosterParser.ParseTeamPage(Html($"<div class=\"info-box\">{box}</div>"));

            teamPage.Staff.ShouldBe([Staff(president, StaffRoleNames.President)]);
        }

        [Theory]
        [InlineData("<div class=\"info-box\"><p><strong>Wiceprezes:</strong> Jan Kowalski</p></div>")]
        [InlineData("<div class=\"info-box\"><p><strong>P. Zarządu ds. Organizacyjnych</strong> Jan Kowalski</p></div>")]
        [InlineData("<div class=\"info-box\"><!--p><strong>Prezes Klubu Kibica:</strong> Jan Kowalski</p--></div>")]
        [InlineData("<div class=\"info-box\"><h4 class=\"title\">KLUB KIBICA</h4><div>Prezes: Jan Kowalski</div></div>")]
        public void ParseTeamPage_Should_ReadNoPresident_When_NoManagementLineNamesThePresident(string html)
        {
            var teamPage = PlsRosterParser.ParseTeamPage(Html(html));

            teamPage.Staff.ShouldBeEmpty();
        }

        private static StaffMemberRecord Staff(string fullName, string role)
        {
            return new StaffMemberRecord { FullName = fullName, Role = role };
        }

        private static HtmlDocument Html(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }
    }
}
