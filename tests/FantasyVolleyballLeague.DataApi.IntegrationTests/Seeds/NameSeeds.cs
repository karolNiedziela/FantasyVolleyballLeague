using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Seeds
{
    internal static class NameSeeds
    {
        public static Player Player(
            string fullName,
            LeagueGender? gender = null,
            DateOnly? dateOfBirth = null,
            int? heightCm = null,
            int? weightKg = null,
            int? attackReachCm = null)
        {
            var player = new Player((gender ?? LeagueGender.Men).Id, fullName, externalId: null);
            player.UpdateProfile(fullName, dateOfBirth, heightCm, weightKg, attackReachCm);

            return player;
        }

        public static Club Club(League league, string name)
        {
            return new Club(name, league.GenderId);
        }

        public static Roster Roster(League league, Season season, Club club, string name, int? externalId = null)
        {
            return new Roster(name, league.Id, season.Id, club.Id, externalId);
        }

        public static RosterPlayer RosterPlayer(
            Roster roster,
            Player player,
            int? shirtNumber = null,
            PlayerPosition? position = null,
            DateOnly? memberFrom = null,
            DateOnly? memberTo = null)
        {
            return new RosterPlayer(roster.Id, player.Id, shirtNumber, position?.Id, memberFrom, memberTo);
        }

        public static StaffMember StaffMember(string fullName)
        {
            return new StaffMember(fullName);
        }

        public static RosterStaffMember RosterStaffMember(Roster roster, StaffMember staffMember, StaffRole role)
        {
            return new RosterStaffMember(roster.Id, staffMember.Id, role.Id);
        }
    }
}
