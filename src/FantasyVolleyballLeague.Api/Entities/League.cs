namespace FantasyVolleyballLeague.Api.Entities
{
    internal sealed class League
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Country { get; set; }

        private League() { }

        public League(string name, string? country = null)
        {
            Name = name;
            Country = country;
        }
    }
}
