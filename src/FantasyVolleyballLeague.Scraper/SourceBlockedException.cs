namespace FantasyVolleyballLeague.Scraper
{
    /// <summary>
    /// The website refused the request because the client sent too many of them (e.g. a Cloudflare block page).
    /// It says nothing about the match itself, so it does not count as a failed attempt of that match.
    /// </summary>
#pragma warning disable S3871 // Exception types should be "public"
    internal sealed class SourceBlockedException : Exception
#pragma warning restore S3871 // Exception types should be "public"

    {
        public SourceBlockedException()
        {
        }

        public SourceBlockedException(string message)
            : base(message)
        {
        }

        public SourceBlockedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
