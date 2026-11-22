using FantasyVolleyballLeague.Shared.Types;

namespace FantasyVolleyballLeague.Shared.UnitTests.Types
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
