namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class Round : Entity
    {
        public string Stage { get; private set; } = string.Empty;

        public int StageOrder { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public int Order { get; private set; }

        public Guid PhaseId { get; private set; }

        public Phase Phase { get; private set; } = default!;

        private Round() { }

        public Round(Guid phaseId, string stage, int stageOrder, string name, int order)
        {
            PhaseId = phaseId;
            Stage = stage;
            StageOrder = stageOrder;
            Name = name;
            Order = order;
        }

        public void Reorder(int stageOrder, int order)
        {
            StageOrder = stageOrder;
            Order = order;
        }
    }
}
