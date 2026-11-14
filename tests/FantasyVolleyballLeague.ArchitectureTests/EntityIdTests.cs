using FantasyVolleyballLeague.ArchitectureTests.Fixtures;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    /// <summary>
    /// Every entity id is a time-ordered UUID v7: entities derive from <see cref="Entity"/>, which assigns it.
    /// </summary>
    public sealed class EntityIdTests
    {
        [Fact]
        public void EntityAssignsUuidV7Id()
        {
            new TestEntity().Id.Version.ShouldBe(7);
        }

        [Fact]
        public void EntityIdCreatesUuidV7()
        {
            EntityId.New().Version.ShouldBe(7);
        }

        [Fact]
        public void EveryDomainTypeWithGuidIdDerivesFromEntity()
        {
            var violations = ProjectTypes.VolleyballData.InNamespace("Domain")
                .Where(type => type.IsClass && type != typeof(Entity))
                .Where(type => type.GetProperty(nameof(Entity.Id))?.PropertyType == typeof(Guid))
                .Where(type => !type.IsSubclassOf(typeof(Entity)) || type.GetProperty(nameof(Entity.Id))!.DeclaringType != typeof(Entity))
                .Select(type => type.Name)
                .ToList();

            violations.ShouldBeEmpty();
        }

        [Fact]
        public void DomainContainsEntities()
        {
            ProjectTypes.VolleyballData.InNamespace("Domain").ShouldContain(type => type.IsSubclassOf(typeof(Entity)));
        }

        private sealed class TestEntity : Entity;
    }
}
