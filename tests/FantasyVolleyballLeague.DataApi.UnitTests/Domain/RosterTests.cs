using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Domain
{
    public sealed class RosterTests
    {
        [Fact]
        public void Constructor_Should_SetNormalizedName()
        {
            var roster = new Roster("ASSECO RESOVIA S.S.A.", EntityId.New(), EntityId.New(), EntityId.New(), 5);

            roster.NormalizedName.ShouldBe("asseco resovia ssa");
        }

        [Fact]
        public void Rename_Should_RefreshNormalizedName()
        {
            var roster = new Roster("Asseco Resovia", EntityId.New(), EntityId.New(), EntityId.New(), 5);

            roster.Rename("Asseco Resovia Rzeszów");

            roster.Name.ShouldBe("Asseco Resovia Rzeszów");
            roster.NormalizedName.ShouldBe("asseco resovia rzeszow");
        }

        [Fact]
        public void RefreshNormalizedName_Should_RecomputeTheValue()
        {
            var roster = new Roster("Łuczniczka Bydgoszcz", EntityId.New(), EntityId.New(), EntityId.New(), null);

            roster.RefreshNormalizedName();

            roster.NormalizedName.ShouldBe("luczniczka bydgoszcz");
        }

        [Fact]
        public void AssignClub_Should_ChangeTheClub()
        {
            var club = EntityId.New();
            var roster = new Roster("LUK Politechnika Lublin", EntityId.New(), EntityId.New(), EntityId.New(), 46);

            roster.AssignClub(club);

            roster.ClubId.ShouldBe(club);
        }
    }
}
