using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class CareerMembershipTests
    {
        private const int Radom = 83;
        private const int Visla = 15;

        [Fact]
        public void ForClubReturnsTheDatesOfTheStintInThatClub()
        {
            var career = new[]
            {
                Entry(Radom, new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30)),
                Entry(Visla, new DateOnly(2020, 1, 30), new DateOnly(2020, 3, 25)),
            };

            CareerMembership.ForClub(career, Radom).ShouldBe((new DateOnly(2019, 10, 25), new DateOnly(2020, 1, 30)));
            CareerMembership.ForClub(career, Visla).ShouldBe((new DateOnly(2020, 1, 30), new DateOnly(2020, 3, 25)));
        }

        [Fact]
        public void ForClubSpansTwoStintsInTheSameClub()
        {
            var career = new[]
            {
                Entry(Radom, new DateOnly(2019, 9, 1), new DateOnly(2019, 11, 1)),
                Entry(Radom, new DateOnly(2020, 1, 10), new DateOnly(2020, 5, 1)),
            };

            CareerMembership.ForClub(career, Radom).ShouldBe((new DateOnly(2019, 9, 1), new DateOnly(2020, 5, 1)));
        }

        [Fact]
        public void ForClubWithoutAStintReturnsUnknownDates()
        {
            CareerMembership.ForClub([], Radom).ShouldBe((null, null));
            CareerMembership.ForClub([Entry(Visla, new DateOnly(2020, 1, 1), new DateOnly(2020, 2, 1))], Radom).ShouldBe((null, null));
        }

        private static CareerEntryFile Entry(int clubId, DateOnly from, DateOnly to)
        {
            return new CareerEntryFile { ClubExternalId = clubId, From = from, To = to };
        }
    }
}
