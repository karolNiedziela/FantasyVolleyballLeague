using FantasyVolleyballLeague.Shared.Constants;

namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class PlayerPosition
    {
        public static readonly PlayerPosition OutsideHitter = new(1, "outside_hitter", PositionNames.OutsideHitter, "OH");
        public static readonly PlayerPosition OppositeHitter = new(2, "opposite_hitter", PositionNames.OppositeHitter, "OPP");
        public static readonly PlayerPosition MiddleBlocker = new(3, "middle_blocker", PositionNames.MiddleBlocker, "MB");
        public static readonly PlayerPosition Setter = new(4, "setter", PositionNames.Setter, "S");
        public static readonly PlayerPosition Libero = new(5, "libero", PositionNames.Libero, "L");

        public int Id { get; private set; }

        /// <summary>
        /// The value the API uses for this position. Part of the public contract, so it never changes.
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;

        public string Abbreviation { get; private set; } = string.Empty;

        private PlayerPosition() { }

        private PlayerPosition(int id, string code, string name, string abbreviation)
        {
            Id = id;
            Code = code;
            Name = name;
            Abbreviation = abbreviation;
        }

        public static PlayerPosition? FromId(int id) => All.FirstOrDefault(p => p.Id == id);

        public static PlayerPosition? FromCode(string code)
        {
            return All.FirstOrDefault(p => string.Equals(p.Code, code, StringComparison.Ordinal));
        }

        public static IReadOnlyCollection<PlayerPosition> All =>
            [OutsideHitter, MiddleBlocker, OppositeHitter, Setter, Libero];
    }
}
