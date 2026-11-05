using FantasyVolleyballLeague.Api.Domain;

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
            Assert.Equal(7, new TestEntity().Id.Version);
        }

        [Fact]
        public void EntityIdCreatesUuidV7()
        {
            Assert.Equal(7, EntityId.New().Version);
        }

        [Fact]
        public void EveryDomainTypeWithGuidIdDerivesFromEntity()
        {
            var violations = ApiTypes.InNamespace("Domain")
                .Where(type => type.IsClass && type != typeof(Entity))
                .Where(type => type.GetProperty(nameof(Entity.Id))?.PropertyType == typeof(Guid))
                .Where(type => !type.IsSubclassOf(typeof(Entity)) || type.GetProperty(nameof(Entity.Id))!.DeclaringType != typeof(Entity))
                .Select(type => type.Name)
                .ToList();

            Assert.Empty(violations);
        }

        [Fact]
        public void DomainContainsEntities()
        {
            Assert.Contains(ApiTypes.InNamespace("Domain"), type => type.IsSubclassOf(typeof(Entity)));
        }

        private sealed class TestEntity : Entity;
    }
}
