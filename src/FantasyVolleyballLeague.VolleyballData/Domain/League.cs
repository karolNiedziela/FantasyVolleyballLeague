namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class League : Entity
    {
        public string Name { get; private set; } = string.Empty;

        public short DivisionNumber { get; private set; }

        public int CountryId { get; private set; }

        public LeagueCountry Country { get; private set; } = default!;

        public int GenderId { get; private set; }

        public LeagueGender Gender { get; private set; } = default!;

        private League() { }

        public League(string name, short divisionNumber, int countryId, int genderId)
        {
            Name = name;
            DivisionNumber = divisionNumber;
            CountryId = countryId;
            GenderId = genderId;
        }

        public void Rename(string name)
        {
            Name = name;
        }
    }
}
