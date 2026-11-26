using FantasyVolleyballLeague.Shared.Text;

namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    /// <summary>
    /// A coach or club official. The league websites give no id for them, so a person is identified by
    /// <see cref="NormalizedName"/>: two people with the same name are one staff member.
    /// </summary>
    internal sealed class StaffMember : Entity, ISearchableName
    {
        public string FullName { get; private set; } = string.Empty;

        public string NormalizedName { get; private set; } = string.Empty;

        private StaffMember() { }

        public StaffMember(string fullName)
        {
            FullName = fullName;
            NormalizedName = NameNormalizer.Normalize(fullName);
        }
    }
}
