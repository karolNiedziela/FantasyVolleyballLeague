using Asp.Versioning;

namespace FantasyVolleyballLeague.DataApi.Common.Versioning
{
    internal static class ApiVersions
    {
        public static readonly ApiVersion V1 = new(1);

        public const string V1DocumentName = "v1";
    }
}
