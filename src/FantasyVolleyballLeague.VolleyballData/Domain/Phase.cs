namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class Phase : Entity
    {
        public string Name { get; private set; } = string.Empty;

        public int Order { get; private set; }

        public Guid SeasonId { get; private set; }

        public Season Season { get; private set; } = default!;

        private Phase() { }

        public Phase(string name, int order, Guid seasonId)
        {
            Name = name;
            Order = order;
            SeasonId = seasonId;
        }

        public void Reorder(int order)
        {
            Order = order;
        }
    }
}
