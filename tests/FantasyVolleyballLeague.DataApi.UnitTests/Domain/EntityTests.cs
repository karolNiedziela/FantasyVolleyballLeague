using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Domain
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
