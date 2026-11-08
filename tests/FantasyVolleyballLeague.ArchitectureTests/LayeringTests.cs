using FantasyVolleyballLeague.DataApi.Common.Endpoints;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    /// <summary>
    /// Rules from <c>.claude/rules/architecture.md</c>.
    /// </summary>
    public sealed class LayeringTests
    {
        [Theory]
        [InlineData("FantasyVolleyballLeague.VolleyballData.Database")]
        [InlineData("FantasyVolleyballLeague.DataApi")]
        public void DomainDoesNotDependOn(string forbiddenNamespace)
        {
            var violations = ProjectTypes.VolleyballData.InNamespace("Domain")
                .SelectMany(type => ProjectTypes.SignatureDependencies(type)
                    .Where(dependency => dependency.Namespace?.StartsWith(forbiddenNamespace, StringComparison.Ordinal) == true)
                    .Select(dependency => $"{type.Name} -> {dependency.FullName}"))
                .ToList();

            Assert.Empty(violations);
        }

        [Fact]
        public void NoMvcControllers()
        {
            var controllers = ProjectTypes.DataApi.Assembly.GetTypes()
                .Where(type => BaseTypes(type).Any(baseType => baseType.FullName is "Microsoft.AspNetCore.Mvc.ControllerBase"))
                .Select(type => type.FullName)
                .ToList();

            Assert.Empty(controllers);
        }

        [Fact]
        public void EndpointsImplementIEndpoint()
        {
            var violations = ProjectTypes.DataApi.Assembly.GetTypes()
                .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith("Endpoint", StringComparison.Ordinal))
                .Where(type => !typeof(IEndpoint).IsAssignableFrom(type))
                .Select(type => type.FullName)
                .ToList();

            Assert.Empty(violations);
        }

        private static IEnumerable<Type> BaseTypes(Type type)
        {
            for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
            {
                yield return baseType;
            }
        }
    }
}
