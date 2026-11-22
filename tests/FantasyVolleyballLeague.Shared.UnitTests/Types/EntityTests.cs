using FantasyVolleyballLeague.Shared.Types;

namespace FantasyVolleyballLeague.Shared.UnitTests.Types
{
    public sealed class EntityTests
    {
        [Fact]
        public void Constructor_Should_AssignUuidV7Id()
        {
            var entity = new TestEntity();

            entity.Id.Version.ShouldBe(7);
        }

        private sealed class TestEntity : Entity;
    }
}
