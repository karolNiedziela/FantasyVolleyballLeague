using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.DataApi.Common.Handlers;
using FantasyVolleyballLeague.VolleyballData.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FantasyVolleyballLeague.ArchitectureTests.Fixtures
{
    /// <summary>
    /// The production assemblies loaded once for all architecture tests, and the groups of types the rules talk about.
    /// </summary>
    internal static class ArchitectureFixture
    {
        public const string DomainNamespace = @"^FantasyVolleyballLeague\.VolleyballData\.Domain(\..*)?$";

        public const string DatabaseNamespace = @"^FantasyVolleyballLeague\.VolleyballData\.Database(\..*)?$";

        public const string FeaturesNamespace = @"^FantasyVolleyballLeague\.DataApi\.Features(\..*)?$";

        public static Architecture Model { get; } = new ArchLoader()
            .LoadAssemblies(typeof(IEndpoint).Assembly, typeof(Entity).Assembly)
            .Build();

        public static IObjectProvider<Class> Handlers { get; } = Classes()
            .That().ImplementInterface(typeof(IQueryHandler<,>))
            .As("Query handlers");

        public static IObjectProvider<Class> DomainClasses { get; } = Classes()
            .That().ResideInNamespaceMatching(DomainNamespace)
            .As("Domain classes");
    }
}
