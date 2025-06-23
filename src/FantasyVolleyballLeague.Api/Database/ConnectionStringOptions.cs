namespace FantasyVolleyballLeague.Api.Database
{
    internal sealed class ConnectionStringOptions
    {
        public const string SectionName = "FantasyVolleyballLeagueDB";

        public string Value { get; }

        public ConnectionStringOptions(string value)
        {
            Value = value;
        }

        public static implicit operator string(ConnectionStringOptions connectionString) => connectionString.Value;
    }
}
