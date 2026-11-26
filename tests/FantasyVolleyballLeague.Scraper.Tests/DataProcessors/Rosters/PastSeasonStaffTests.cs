using FantasyVolleyballLeague.Scraper.DataProcessors.Rosters;
using FantasyVolleyballLeague.Scraper.Models;
using FantasyVolleyballLeague.Shared.Constants;

namespace FantasyVolleyballLeague.Scraper.Tests.DataProcessors.Rosters
{
    public sealed class PastSeasonStaffTests
    {
        private static readonly StaffMemberRecord PageCoach = Member("Jan Kowalski", StaffRoleNames.HeadCoach);
        private static readonly StaffMemberRecord PagePresident = Member("Anna Nowak", StaffRoleNames.President);
        private static readonly StaffMemberRecord StoredCoach = Member("Piotr Wiśniewski", StaffRoleNames.HeadCoach);
        private static readonly StaffMemberRecord StoredPresident = Member("Maria Wójcik", StaffRoleNames.President);

        [Fact]
        public void KeepStoredPresident_Should_TakeThePresidentFromTheStoredFile()
        {
            var roster = PastSeasonStaff.KeepStoredPresident(Roster(PageCoach, PagePresident), Roster(StoredCoach, StoredPresident));

            roster.Staff.Where(member => member.Role == StaffRoleNames.President).ShouldBe([StoredPresident]);
        }

        [Fact]
        public void KeepStoredPresident_Should_TakeTheCoachesFromThePage()
        {
            var roster = PastSeasonStaff.KeepStoredPresident(Roster(PageCoach, PagePresident), Roster(StoredCoach, StoredPresident));

            roster.Staff.Where(member => member.Role != StaffRoleNames.President).ShouldBe([PageCoach]);
        }

        [Fact]
        public void KeepStoredPresident_Should_LeaveNoPresident_When_NoFileIsStored()
        {
            var roster = PastSeasonStaff.KeepStoredPresident(Roster(PageCoach, PagePresident), stored: null);

            roster.Staff.ShouldBe([PageCoach]);
        }

        private static StaffMemberRecord Member(string fullName, string role)
        {
            return new StaffMemberRecord { FullName = fullName, Role = role };
        }

        private static RosterRecord Roster(params StaffMemberRecord[] staff)
        {
            return new RosterRecord
            {
                ScrapedAtUtc = new DateTime(2026, 11, 25, 0, 0, 0, DateTimeKind.Utc),
                League = "PlusLiga",
                Season = new SeasonReference(2024, 2025, 47),
                Club = new ClubLink(2, "Aluron CMC Warta Zawiercie"),
                Staff = staff,
            };
        }
    }
}
