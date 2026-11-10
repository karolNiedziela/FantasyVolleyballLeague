using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class RosterMemberTests
    {
        private static readonly DateOnly From = new(2019, 10, 25);
        private static readonly DateOnly To = new(2020, 1, 30);

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

        [Fact]
        public void CoversDateWithUnknownDatesCoversEverything()
        {
            Member(null, null).CoversDate(new DateOnly(2000, 1, 1)).ShouldBeTrue();
            Member(From, null).CoversDate(new DateOnly(2030, 1, 1)).ShouldBeTrue();
            Member(null, To).CoversDate(new DateOnly(1999, 1, 1)).ShouldBeTrue();
        }

        private static RosterMember Member(DateOnly? from, DateOnly? to)
        {
            return new RosterMember(Guid.CreateVersion7(), 10, "FILIP MICHAL", from, to);
        }
    }
}
