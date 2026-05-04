namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class PlayerPosition
    {
        public static readonly PlayerPosition OutsideHitter = new(1, "Outside Hitter", "OH");
        public static readonly PlayerPosition OppositeHitter = new(2, "Opposite Hitter", "OPP");
        public static readonly PlayerPosition MiddleBlocker = new(3, "Middle Blocker", "MB");
        public static readonly PlayerPosition Setter = new(4, "Setter", "S");
        public static readonly PlayerPosition Libero = new(5, "Libero", "L");

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string Abbreviation { get; private set; } = string.Empty;

        private PlayerPosition() { }

        private PlayerPosition(int id, string name, string abbreviation)
        {
            Id = id;
            Name = name;
            Abbreviation = abbreviation;
        }

        public static PlayerPosition? FromId(int id) => All.FirstOrDefault(p => p.Id == id);

        public static IReadOnlyCollection<PlayerPosition> All =>
            [OutsideHitter, MiddleBlocker, OppositeHitter, Setter, Libero];
    }
}
