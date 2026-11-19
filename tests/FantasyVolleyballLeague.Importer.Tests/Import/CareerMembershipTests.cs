using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class CareerMembershipTests
    {
        private const int Radom = 83;
        private const int Visla = 15;

        [Fact]
        public void ForClub_Should_ReturnTheDatesOfTheStintInThatClub()
        {
            var career = new[]
            {
                Entry(Radom, new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30)),
                Entry(Visla, new DateOnly(2020, 1, 30), new DateOnly(2020, 3, 25)),
            };

            var membership = CareerMembership.ForClub(career, Visla);

            membership.ShouldBe((new DateOnly(2020, 1, 30), new DateOnly(2020, 3, 25)));
        }

        [Fact]
        public void ForClub_Should_SpanTwoStintsInTheSameClub()
        {
            var career = new[]
            {
                Entry(Radom, new DateOnly(2019, 9, 1), new DateOnly(2019, 11, 1)),
                Entry(Radom, new DateOnly(2020, 1, 10), new DateOnly(2020, 5, 1)),
            };

            var membership = CareerMembership.ForClub(career, Radom);

            membership.ShouldBe((new DateOnly(2019, 9, 1), new DateOnly(2020, 5, 1)));
        }

        [Fact]
        public void ForClub_Should_UseTheEarliestStartAndTheLatestEnd_When_TheStintsAreNotInOrder()
        {
            var career = new[]
            {
                Entry(Radom, new DateOnly(2020, 1, 10), new DateOnly(2020, 5, 1)),
                Entry(Radom, new DateOnly(2019, 9, 1), new DateOnly(2019, 11, 1)),
            };

            var membership = CareerMembership.ForClub(career, Radom);

            membership.ShouldBe((new DateOnly(2019, 9, 1), new DateOnly(2020, 5, 1)));
        }

        [Fact]
        public void ForClub_Should_ReturnUnknownDates_When_TheCareerIsEmpty()
        {
            var membership = CareerMembership.ForClub([], Radom);

            membership.ShouldBe((null, null));
        }

        [Fact]
        public void ForClub_Should_ReturnUnknownDates_When_TheClubIsNotInTheCareer()
        {
            var career = new[] { Entry(Visla, new DateOnly(2020, 1, 1), new DateOnly(2020, 2, 1)) };

            var membership = CareerMembership.ForClub(career, Radom);

            membership.ShouldBe((null, null));
        }

        private static CareerEntryFile Entry(int clubId, DateOnly from, DateOnly to)
        {
            return new CareerEntryFile { ClubExternalId = clubId, From = from, To = to };
        }
    }
}
