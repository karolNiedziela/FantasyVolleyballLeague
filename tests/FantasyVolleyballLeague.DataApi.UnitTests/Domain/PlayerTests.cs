using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Domain
{
    public sealed class PlayerTests
    {
        [Fact]
        public void Constructor_Should_SetNormalizedName()
        {
            var player = new Player(LeagueGender.Women.Id, "Anna Rybak-Czyrniańska", 12);

            player.NormalizedName.ShouldBe("anna rybak czyrnianska");
        }

        [Fact]
        public void UpdateProfile_Should_RefreshNormalizedName_When_NameChanges()
        {
            var player = new Player(LeagueGender.Men.Id, "Lukasz Kaczmarek", 1);

            player.UpdateProfile("Łukasz Kaczmarek", null, null, null, null);

            player.FullName.ShouldBe("Łukasz Kaczmarek");
            player.NormalizedName.ShouldBe("lukasz kaczmarek");
        }

        [Fact]
        public void RefreshNormalizedName_Should_RecomputeTheValue()
        {
            var player = new Player(LeagueGender.Men.Id, "Jakub Szymański", 2);

            player.RefreshNormalizedName();

            player.NormalizedName.ShouldBe("jakub szymanski");
        }
    }
}
