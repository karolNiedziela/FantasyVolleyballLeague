namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class League
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public short DivisionNumber { get; private set; }

        public int CountryId { get; private set; }

        public LeagueCountry Country { get; private set; } = default!;

        private League() { }

        public League(string name, short divisionNumber, int countryId)
        {
            Id = Guid.CreateVersion7();
            Name = name;
            DivisionNumber = divisionNumber;
            CountryId = countryId;
        }
    }
}
