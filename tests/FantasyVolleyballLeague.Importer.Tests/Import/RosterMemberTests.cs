using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class RosterMemberTests
    {
        private static readonly DateOnly From = new(2019, 10, 25);
        private static readonly DateOnly To = new(2020, 1, 30);
        private static readonly Guid PlayerId = new("00000000-0000-0000-0000-000000000001");

        [Theory]
        [InlineData(2019, 10, 25)]
        [InlineData(2019, 12, 1)]
        [InlineData(2020, 1, 30)]
        public void CoversDateIncludesTheWholeStint(int year, int month, int day)
        {
            Member(From, To).CoversDate(new DateOnly(year, month, day)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(2019, 10, 24)]
        [InlineData(2020, 1, 31)]
        public void CoversDateToleratesOneDayOutsideTheStint(int year, int month, int day)
        {
            Member(From, To).CoversDate(new DateOnly(year, month, day)).ShouldBeTrue();
        }

        [Theory]
        [InlineData(2019, 10, 23)]
        [InlineData(2020, 2, 1)]
        public void CoversDateRejectsMoreThanOneDayOutsideTheStint(int year, int month, int day)
        {
            Member(From, To).CoversDate(new DateOnly(year, month, day)).ShouldBeFalse();
        }

        public static TheoryData<DateOnly?, DateOnly?, DateOnly, bool> UnknownDateCases { get; } = new()
        {
            { null, null, new DateOnly(2000, 1, 1), true },
            { From, null, new DateOnly(2030, 1, 1), true },
            { null, To, new DateOnly(1999, 1, 1), true },
            { From, null, new DateOnly(2019, 10, 23), false },
            { null, To, new DateOnly(2020, 2, 1), false },
        };

        [Theory]
        [MemberData(nameof(UnknownDateCases))]
        public void CoversDate_Should_TreatAnUnknownBoundAsOpen_And_StillApplyTheKnownOne(DateOnly? from, DateOnly? to, DateOnly date, bool expected)
        {
            Member(from, to).CoversDate(date).ShouldBe(expected);
        }

        private static RosterMember Member(DateOnly? from, DateOnly? to)
        {
            return new RosterMember(PlayerId, 10, "FILIP MICHAL", from, to);
        }
    }
}
