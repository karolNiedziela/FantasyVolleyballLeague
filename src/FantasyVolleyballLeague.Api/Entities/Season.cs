namespace FantasyVolleyballLeague.Api.Entities
{
    internal sealed class Season
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int StartYearNumber { get; set; }

        public int EndYearNumber { get; set; }

        private Season() { }

        public Season(string name, int startYearNumber, int endYearNumber)
        {
            Name = name;
            StartYearNumber = startYearNumber;
            EndYearNumber = endYearNumber;
        }
    }
}
