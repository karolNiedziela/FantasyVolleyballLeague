using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Dependencies;
using ArchUnitNET.xUnitV3;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Database;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static FantasyVolleyballLeague.ArchitectureTests.Fixtures.ArchitectureFixture;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    public sealed class HandlerTests
    {
        private static readonly string QueryHandlerInterfaceName = typeof(IQueryHandler<,>).FullName!.Split('`')[0];

        [Fact]
        public void Handlers_Should_BeSealed()
        {
            Classes().That().Are(Handlers)
                .Should().BeSealed()
                .Check(Model);
        }

        [Fact]
        public void Handlers_Should_BeNamedHandler()
        {
            Classes().That().Are(Handlers)
                .Should().HaveNameEndingWith("Handler")
                .Check(Model);
        }

        [Fact]
        public void Handlers_Should_LiveInFeatures()
        {
            Classes().That().Are(Handlers)
                .Should().ResideInNamespaceMatching(FeaturesNamespace)
                .Check(Model);
        }

        [Fact]
        public void Handlers_Should_NotDependOnOtherHandlers()
        {
            Classes().That().Are(Handlers)
                .Should().FollowCustomCondition(
                    handler => !DependsOnAnotherHandler(handler),
                    "not depend on another handler",
                    "depends on another handler")
                .Check(Model);
        }

        [Fact]
        public void Handlers_Should_ReadTheDatabaseThroughIDbContext()
        {
            Classes().That().Are(Handlers)
                .Should().NotDependOnAny(typeof(FantasyVolleyballLeagueDbContext))
                .Check(Model);
        }

        private static bool DependsOnAnotherHandler(Class handler)
        {
            var handlers = Handlers.GetObjects(Model).ToList();

            return handler.Dependencies
                .Where(dependency => dependency is not ImplementsInterfaceDependency)
                .Select(dependency => dependency.Target)
                .Any(target => (target != handler && handlers.Contains(target))
                    || target.FullName.StartsWith(QueryHandlerInterfaceName, StringComparison.Ordinal));
        }
    }
}
