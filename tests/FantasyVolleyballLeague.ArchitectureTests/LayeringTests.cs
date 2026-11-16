using FantasyVolleyballLeague.ArchitectureTests.Fixtures;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;

namespace FantasyVolleyballLeague.ArchitectureTests
{
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

            violations.ShouldBeEmpty();
        }

        [Fact]
        public void EndpointsImplementIEndpoint()
        {
            var violations = ProjectTypes.DataApi.Assembly.GetTypes()
                .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith("Endpoint", StringComparison.Ordinal))
                .Where(type => !typeof(IEndpoint).IsAssignableFrom(type))
                .Select(type => type.FullName)
                .ToList();

            violations.ShouldBeEmpty();
        }
    }
}
