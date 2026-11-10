using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Rosters;
using FantasyVolleyballLeague.Scraper.Tests.Fixtures;

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
            (krulicki.FullName, krulicki.ShirtNumber, krulicki.Position).ShouldBe(("Bartłomiej Krulicki", 1, "Middle Blocker"));
            var szymanski = teamPage.Players.Where(player => player.ExternalId == 837).ShouldHaveSingleItem();
            (szymanski.FullName, szymanski.ShirtNumber, szymanski.Position).ShouldBe(("Jakub Szymański", 2, "Outside Hitter"));
        }

        [Fact]
        public void ParsePlayerProfileReadsValuesByLabel()
        {
            var details = PlsRosterParser.ParsePlayerProfile(Fixture.Load("player_3169_tour53.html"));

            details.ShouldBe(new PlsPlayerDetails(new DateOnly(2000, 1, 18), "Setter", 195, 91, 319, 8));
        }

        [Fact]
        public void ParseCareerReadsEveryStintWithClubAndSeason()
        {
            var career = PlsRosterParser.ParseCareer(Fixture.Load("player_1668_tour32.html"));

            career.Count.ShouldBe(9);
            var opposite = PlsPositionMapper.Map("Atakujący");
            career.ShouldContain(new PlsCareerRow(83, 32, new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30), 10, opposite));
            career.ShouldContain(new PlsCareerRow(15, 32, new DateOnly(2020, 1, 30), new DateOnly(2020, 3, 25), 94, opposite));
            career.ShouldContain(new PlsCareerRow(68, 35, new DateOnly(2020, 9, 11), new DateOnly(2021, 5, 9), 94, opposite));
        }

        [Fact]
        public void ToCareerEntriesKeepsOnlyTheSeasonOfTheRosterOldestFirst()
        {
            var career = PlsRosterParser.ParseCareer(Fixture.Load("player_1668_tour32.html"));

            var entries = PlsRosterScrapper.ToCareerEntries(career, 32);

            entries.Select(entry => entry.ClubExternalId).ShouldBe([83, 15]);
            entries[0].From.ShouldBe(new DateOnly(2019, 10, 25));
            entries[1].To.ShouldBe(new DateOnly(2020, 3, 25));
        }

        [Fact]
        public void ParseCareerWithoutCareerTableReturnsNothing()
        {
            PlsRosterParser.ParseCareer(Fixture.Load("teams_tour53.html")).ShouldBeEmpty();
        }
    }
}
