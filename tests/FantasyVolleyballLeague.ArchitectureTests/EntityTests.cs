using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.xUnitV3;
using FantasyVolleyballLeague.VolleyballData.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static FantasyVolleyballLeague.ArchitectureTests.Fixtures.ArchitectureFixture;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    public sealed class EntityTests
    {
        [Fact]
        public void DomainClassesWithGuidId_Should_DeriveFromEntity()
        {
            Classes().That().Are(DomainClasses)
                .And().FollowCustomPredicate(
                    type => type.GetPropertyMembers().Any(property => property.Name == nameof(Entity.Id) && property.Type.FullName == typeof(Guid).FullName),
                    "have a Guid Id")
                .Should().BeAssignableTo(typeof(Entity))
                .Check(Model);
        }
    }
}
