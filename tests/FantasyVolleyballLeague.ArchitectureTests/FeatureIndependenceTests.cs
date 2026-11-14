using FantasyVolleyballLeague.ArchitectureTests.Fixtures;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    public sealed class FeatureIndependenceTests
    {
        private const string FeaturesNamespace = "FantasyVolleyballLeague.DataApi.Features";

        [Fact]
        public void FeaturesDoNotDependOnOtherFeatures()
        {
            var violations = ProjectTypes.DataApi.InNamespace("Features")
                .SelectMany(type => ProjectTypes.SignatureDependencies(type)
                    .Where(dependency => IsDependencyOnOtherFeature(type.Namespace, dependency.Namespace))
                    .Select(dependency => $"{type.FullName} -> {dependency.FullName}"))
                .ToList();

            violations.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(FeaturesNamespace + ".Rosters.GetRosters", FeaturesNamespace + ".Players.GetPlayers", true)]
        [InlineData(FeaturesNamespace + ".Rosters", FeaturesNamespace + ".RostersArchive.GetRosters", true)]
        [InlineData(FeaturesNamespace + ".Rosters.GetRosters", FeaturesNamespace + ".Rosters.GetRoster", false)]
        [InlineData(FeaturesNamespace + ".Rosters.GetRosters", "FantasyVolleyballLeague.DataApi.Common.API.Paging", false)]
        [InlineData(FeaturesNamespace + ".Rosters.GetRosters", "FantasyVolleyballLeague.VolleyballData.Domain", false)]
        [InlineData("FantasyVolleyballLeague.DataApi.Common.API", FeaturesNamespace + ".Players.GetPlayers", false)]
        public void IsDependencyOnOtherFeatureComparesTheFirstSegmentAfterFeatures(
            string ownNamespace,
            string dependencyNamespace,
            bool expected)
        {
            IsDependencyOnOtherFeature(ownNamespace, dependencyNamespace).ShouldBe(expected);
        }

        private static bool IsDependencyOnOtherFeature(string? ownNamespace, string? dependencyNamespace)
        {
            var ownFeature = FeatureOf(ownNamespace);
            var dependencyFeature = FeatureOf(dependencyNamespace);

            return ownFeature is not null
                && dependencyFeature is not null
                && !string.Equals(ownFeature, dependencyFeature, StringComparison.Ordinal);
        }

        private static string? FeatureOf(string? typeNamespace)
        {
            var prefix = $"{FeaturesNamespace}.";
            if (typeNamespace is null || !typeNamespace.StartsWith(prefix, StringComparison.Ordinal))
            {
                return null;
            }

            var relativeNamespace = typeNamespace[prefix.Length..];
            var firstDot = relativeNamespace.IndexOf('.', StringComparison.Ordinal);

            return firstDot < 0 ? relativeNamespace : relativeNamespace[..firstDot];
        }
    }
}
