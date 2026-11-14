using FantasyVolleyballLeague.ArchitectureTests.Fixtures;
using FantasyVolleyballLeague.DataApi.Common.Handlers;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    /// <summary>
    /// Handler rules from <c>.claude/rules/architecture.md</c> (vertical slices).
    /// </summary>
    public sealed class HandlerTests
    {
        [Fact]
        public void HandlersAreSealed()
        {
            var violations = HandlerImplementations()
                .Where(type => !type.IsSealed)
                .Select(type => type.FullName)
                .ToList();

            violations.ShouldBeEmpty();
        }

        [Fact]
        public void HandlersAreNamedHandler()
        {
            var violations = HandlerImplementations()
                .Where(type => !type.Name.EndsWith("Handler", StringComparison.Ordinal))
                .Select(type => type.FullName)
                .ToList();

            violations.ShouldBeEmpty();
        }

        [Fact]
        public void HandlersLiveInFeatures()
        {
            var featureTypes = ProjectTypes.DataApi.InNamespace("Features").ToHashSet();

            var violations = HandlerImplementations()
                .Where(type => !featureTypes.Contains(type))
                .Select(type => type.FullName)
                .ToList();

            violations.ShouldBeEmpty();
        }

        [Fact]
        public void HandlersDoNotDependOnOtherHandlers()
        {
            var violations = HandlerImplementations()
                .SelectMany(type => type.GetConstructors()
                    .SelectMany(constructor => constructor.GetParameters())
                    .Where(parameter => IsHandlerType(parameter.ParameterType))
                    .Select(parameter => $"{type.Name} -> {parameter.ParameterType.Name}"))
                .ToList();

            violations.ShouldBeEmpty();
        }

        private static IEnumerable<Type> HandlerImplementations()
        {
            return ProjectTypes.DataApi.Assembly.GetTypes()
                .Where(type => type is { IsClass: true, IsAbstract: false } && ImplementsQueryHandler(type));
        }

        private static bool IsHandlerType(Type type)
        {
            return IsQueryHandlerInterface(type) || (type.IsClass && ImplementsQueryHandler(type));
        }

        private static bool ImplementsQueryHandler(Type type)
        {
            return type.GetInterfaces().Any(IsQueryHandlerInterface);
        }

        private static bool IsQueryHandlerInterface(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryHandler<,>);
        }
    }
}
