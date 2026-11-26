using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class RosterStaffChangesTests
    {
        private const int HeadCoach = 1;
        private const int President = 4;

        private static readonly Guid Kowalski = new("00000000-0000-0000-0000-000000000001");
        private static readonly Guid Nowak = new("00000000-0000-0000-0000-000000000002");

        [Fact]
        public void Between_Should_AddTheAssignmentsMissingFromTheStoredStaff()
        {
            var changes = RosterStaffChanges.Between(
                stored: [],
                scraped: [Assignment(Kowalski, HeadCoach)]);

            changes.Added.ShouldBe([Assignment(Kowalski, HeadCoach)]);
        }

        [Fact]
        public void Between_Should_RemoveTheAssignmentsMissingFromTheFile()
        {
            var changes = RosterStaffChanges.Between(
                stored: [Assignment(Kowalski, HeadCoach), Assignment(Nowak, President)],
                scraped: [Assignment(Kowalski, HeadCoach)]);

            changes.Removed.ShouldBe([Assignment(Nowak, President)]);
        }

        [Fact]
        public void Between_Should_ChangeNothing_When_TheStaffIsTheSame()
        {
            var changes = RosterStaffChanges.Between(
                stored: [Assignment(Kowalski, HeadCoach)],
                scraped: [Assignment(Kowalski, HeadCoach)]);

            changes.ShouldSatisfyAllConditions(
                () => changes.Added.ShouldBeEmpty(),
                () => changes.Removed.ShouldBeEmpty());
        }

        [Fact]
        public void Between_Should_KeepTwoRolesOfOnePersonApart()
        {
            var changes = RosterStaffChanges.Between(
                stored: [Assignment(Kowalski, HeadCoach)],
                scraped: [Assignment(Kowalski, HeadCoach), Assignment(Kowalski, President)]);

            changes.Added.ShouldBe([Assignment(Kowalski, President)]);
        }

        [Fact]
        public void Between_Should_AddARepeatedAssignmentOnce()
        {
            var changes = RosterStaffChanges.Between(
                stored: [],
                scraped: [Assignment(Kowalski, HeadCoach), Assignment(Kowalski, HeadCoach)]);

            changes.Added.ShouldBe([Assignment(Kowalski, HeadCoach)]);
        }

        private static RosterStaffAssignment Assignment(Guid staffMemberId, int roleId)
        {
            return new RosterStaffAssignment(staffMemberId, roleId);
        }
    }
}
