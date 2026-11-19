using ArchUnitNET.xUnitV3;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static FantasyVolleyballLeague.ArchitectureTests.Fixtures.ArchitectureFixture;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    public sealed class LayeringTests
    {
        [Fact]
        public void Domain_Should_NotDependOnDatabase()
        {
            Types().That().Are(DomainClasses)
                .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(DatabaseNamespace)
                .Check(Model);
        }

        [Fact]
        public void Endpoints_Should_ImplementIEndpoint()
        {
            Classes().That().HaveNameEndingWith("Endpoint").And().AreNotAbstract()
                .Should().ImplementInterface(typeof(IEndpoint))
                .Check(Model);
        }
    }
}
