using System.Globalization;

namespace FantasyVolleyballLeague.Importer.Import
{
    internal static class StatisticsParsing
    {
        /// <summary>
        /// Parses percentages as written by the league websites: "41%", "41.5%", "41,5%" or "-" (no attempts).
        /// </summary>
        public static decimal? ParsePercent(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim().TrimEnd('%').Replace(',', '.');

            return decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var percent)
                ? percent
                : null;
        }

        /// <summary>
        /// Raw files store UTC times; Npgsql only writes <see cref="DateTimeOffset"/> values with a zero offset.
        /// The value is truncated to microseconds (the precision of PostgreSQL <c>timestamptz</c>), so a value read
        /// back from the database compares equal to the one computed from the file.
        /// </summary>
        public static DateTimeOffset ToUtcOffset(DateTime utc)
        {
            var ticks = utc.Ticks - (utc.Ticks % TicksPerMicrosecond);
            return new DateTimeOffset(new DateTime(ticks, DateTimeKind.Utc));
        }

        private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;
    }
}
