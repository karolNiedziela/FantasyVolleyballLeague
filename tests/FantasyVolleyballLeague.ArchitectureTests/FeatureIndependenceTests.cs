using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static FantasyVolleyballLeague.ArchitectureTests.Fixtures.ArchitectureFixture;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    public sealed class FeatureIndependenceTests
    {
        private const string FeaturesPrefix = "FantasyVolleyballLeague.DataApi.Features.";

        public static TheoryData<string> FeatureNames { get; } = new TheoryData<string>(
            Model.Namespaces
                .Select(ns => ns.FullName)
                .Where(name => name.StartsWith(FeaturesPrefix, StringComparison.Ordinal))
                .Select(name => name[FeaturesPrefix.Length..].Split('.')[0])
                .Distinct());

        [Theory]
        [MemberData(nameof(FeatureNames))]
        public void Feature_Should_NotDependOnOtherFeatures(string feature)
        {
            var escapedPrefix = FeaturesPrefix.Replace(".", @"\.", StringComparison.Ordinal);

            Types().That().ResideInNamespaceMatching($@"^{escapedPrefix}{feature}(\..*)?$")
                .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching($@"^{escapedPrefix}(?!{feature}(\.|$)).+")
                .WithoutRequiringPositiveResults()
                .Check(Model);
        }
    }
}
