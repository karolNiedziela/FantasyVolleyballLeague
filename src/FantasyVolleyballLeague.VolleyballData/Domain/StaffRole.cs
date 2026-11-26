using FantasyVolleyballLeague.Shared.Constants;

namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    internal sealed class StaffRole
    {
        public static readonly StaffRole HeadCoach = new(1, "head_coach", StaffRoleNames.HeadCoach);
        public static readonly StaffRole SecondCoach = new(2, "second_coach", StaffRoleNames.SecondCoach);
        public static readonly StaffRole ThirdCoach = new(3, "third_coach", StaffRoleNames.ThirdCoach);
        public static readonly StaffRole President = new(4, "president", StaffRoleNames.President);

        public int Id { get; private set; }

        /// <summary>
        /// The value the API uses for this role. Part of the public contract, so it never changes.
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        public string Name { get; private set; } = string.Empty;

        private StaffRole() { }

        private StaffRole(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        public static IReadOnlyCollection<StaffRole> All => [HeadCoach, SecondCoach, ThirdCoach, President];

        public static StaffRole? FromName(string name)
        {
            return All.FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static StaffRole? FromCode(string code)
        {
            return All.FirstOrDefault(r => string.Equals(r.Code, code, StringComparison.Ordinal));
        }
    }
}
