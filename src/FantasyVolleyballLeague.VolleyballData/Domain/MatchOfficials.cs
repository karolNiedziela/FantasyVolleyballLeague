namespace FantasyVolleyballLeague.Api.Domain
{
    /// <summary>
    /// Officials are not known for scheduled matches, so every name is optional.
    /// </summary>
    internal sealed record MatchOfficials(string? FirstReferee, string? SecondReferee, string? Commissioner);
}
