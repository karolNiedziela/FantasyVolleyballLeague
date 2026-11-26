using System.Text.RegularExpressions;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Domain
{
    public sealed partial class LookupCodeTests
    {
        public static TheoryData<string> AllCodes =>
        [
            .. LeagueGender.All.Select(x => x.Code),
            .. LeagueCountry.All.Select(x => x.Code),
            .. MatchStatus.All.Select(x => x.Code),
            .. StatisticsLevel.All.Select(x => x.Code),
            .. PlayerPosition.All.Select(x => x.Code),
            .. PhaseType.All.Select(x => x.Code),
            .. StaffRole.All.Select(x => x.Code),
        ];

        [Fact]
        public void LeagueGender_Should_HaveTheContractCodes()
        {
            Codes(LeagueGender.All, x => x.Id, x => x.Code).ShouldBe([(1, "men"), (2, "women")]);
        }

        [Fact]
        public void LeagueCountry_Should_HaveTheContractCodes()
        {
            Codes(LeagueCountry.All, x => x.Id, x => x.Code).ShouldBe([(1, "pl")]);
        }

        [Fact]
        public void MatchStatus_Should_HaveTheContractCodes()
        {
            Codes(MatchStatus.All, x => x.Id, x => x.Code).ShouldBe([(1, "scheduled"), (2, "finished")]);
        }

        [Fact]
        public void StatisticsLevel_Should_HaveTheContractCodes()
        {
            Codes(StatisticsLevel.All, x => x.Id, x => x.Code).ShouldBe([(1, "none"), (2, "lineup_only"), (3, "full")]);
        }

        [Fact]
        public void PlayerPosition_Should_HaveTheContractCodes()
        {
            Codes(PlayerPosition.All, x => x.Id, x => x.Code).ShouldBe(
            [
                (1, "outside_hitter"),
                (2, "opposite_hitter"),
                (3, "middle_blocker"),
                (4, "setter"),
                (5, "libero"),
            ]);
        }

        [Fact]
        public void PhaseType_Should_HaveTheContractCodes()
        {
            Codes(PhaseType.All, x => x.Id, x => x.Code).ShouldBe([(1, "regular_season"), (2, "playoff"), (3, "relegation")]);
        }

        [Fact]
        public void StaffRole_Should_HaveTheContractCodes()
        {
            Codes(StaffRole.All, x => x.Id, x => x.Code).ShouldBe(
            [
                (1, "head_coach"),
                (2, "second_coach"),
                (3, "third_coach"),
                (4, "president"),
            ]);
        }

        [Theory]
        [MemberData(nameof(AllCodes))]
        public void Code_Should_BeSnakeCase(string code)
        {
            SnakeCase().IsMatch(code).ShouldBeTrue();
        }

        private static List<(int Id, string Code)> Codes<T>(IEnumerable<T> lookups, Func<T, int> id, Func<T, string> code)
        {
            return lookups.Select(lookup => (id(lookup), code(lookup))).OrderBy(pair => pair.Item1).ToList();
        }

        [GeneratedRegex("^[a-z]+(_[a-z]+)*$")]
        private static partial Regex SnakeCase();
    }
}
