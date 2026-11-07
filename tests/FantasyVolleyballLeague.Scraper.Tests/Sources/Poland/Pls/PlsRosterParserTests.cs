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

            Assert.Equal(14, clubs.Count);
            Assert.Contains(new ClubLink(2, "Aluron CMC Warta Zawiercie"), clubs);
            Assert.Contains(new ClubLink(3, "Asseco Resovia Rzeszów"), clubs);
        }

        [Fact]
        public void ParseTeamPageReadsRosterEntries()
        {
            var teamPage = PlsRosterParser.ParseTeamPage(Fixture.Load("team_18_tour53.html"));

            Assert.Equal("GKS Katowice", teamPage.Name);
            var krulicki = Assert.Single(teamPage.Players, player => player.ExternalId == 278);
            Assert.Equal(("Bartłomiej Krulicki", 1, "Middle Blocker"), (krulicki.FullName, krulicki.ShirtNumber, krulicki.Position));
            var szymanski = Assert.Single(teamPage.Players, player => player.ExternalId == 837);
            Assert.Equal(("Jakub Szymański", 2, "Outside Hitter"), (szymanski.FullName, szymanski.ShirtNumber, szymanski.Position));
        }

        [Fact]
        public void ParsePlayerProfileReadsValuesByLabel()
        {
            var details = PlsRosterParser.ParsePlayerProfile(Fixture.Load("player_3169_tour53.html"));

            Assert.Equal(new PlsPlayerDetails(new DateOnly(2000, 1, 18), "Setter", 195, 91, 319, 8), details);
        }

        [Fact]
        public void ParseCareerReadsEveryStintWithClubAndSeason()
        {
            var career = PlsRosterParser.ParseCareer(Fixture.Load("player_1668_tour32.html"));

            Assert.Equal(9, career.Count);
            var opposite = PlsPositionMapper.Map("Atakujący");
            Assert.Contains(new PlsCareerRow(83, 32, new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30), 10, opposite), career);
            Assert.Contains(new PlsCareerRow(15, 32, new DateOnly(2020, 1, 30), new DateOnly(2020, 3, 25), 94, opposite), career);
            Assert.Contains(new PlsCareerRow(68, 35, new DateOnly(2020, 9, 11), new DateOnly(2021, 5, 9), 94, opposite), career);
        }

        [Fact]
        public void ToCareerEntriesKeepsOnlyTheSeasonOfTheRosterOldestFirst()
        {
            var career = PlsRosterParser.ParseCareer(Fixture.Load("player_1668_tour32.html"));

            var entries = PlsRosterScrapper.ToCareerEntries(career, 32);

            Assert.Equal([83, 15], entries.Select(entry => entry.ClubExternalId));
            Assert.Equal(new DateOnly(2019, 10, 25), entries[0].From);
            Assert.Equal(new DateOnly(2020, 3, 25), entries[1].To);
        }

        [Fact]
        public void ParseCareerWithoutCareerTableReturnsNothing()
        {
            Assert.Empty(PlsRosterParser.ParseCareer(Fixture.Load("teams_tour53.html")));
        }
    }
}
