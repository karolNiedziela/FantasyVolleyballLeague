namespace FantasyVolleyballLeague.Domain.Entities
{
    public sealed class Season
    {
        public Guid Id { get; set; }

        public string Name { get; private set; } = string.Empty;

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        private Season() { }

        public Season(int startYear, int endYear, Guid id)
        {
            Id = id;
            Name = $"{startYear}/{endYear}";
            StartYear = startYear;
            EndYear = endYear;
        }
    }
}
