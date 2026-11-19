using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Domain
{
    public sealed class EntityIdTests
    {
        [Fact]
        public void New_Should_CreateUuidV7()
        {
            var id = EntityId.New();

            id.Version.ShouldBe(7);
        }
    }
}
