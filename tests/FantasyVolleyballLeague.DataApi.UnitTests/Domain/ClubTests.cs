using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Domain
{
    public sealed class ClubTests
    {
        [Fact]
        public void Constructor_Should_SetNormalizedName()
        {
            var club = new Club("ŚLEPSK MALOW SUWAŁKI", LeagueGender.Men.Id);

            club.NormalizedName.ShouldBe("slepsk malow suwalki");
        }

        [Fact]
        public void Rename_Should_RefreshNormalizedName()
        {
            var club = new Club("Exact Systems Norwid Częstochowa", LeagueGender.Men.Id);

            club.Rename("NEONET Norwid Częstochowa");

            club.Name.ShouldBe("NEONET Norwid Częstochowa");
            club.NormalizedName.ShouldBe("neonet norwid czestochowa");
        }

        [Fact]
        public void RefreshNormalizedName_Should_RecomputeTheValue()
        {
            var club = new Club("Łuczniczka Bydgoszcz", LeagueGender.Men.Id);

            club.RefreshNormalizedName();

            club.NormalizedName.ShouldBe("luczniczka bydgoszcz");
        }
    }
}
