namespace FantasyVolleyballLeague.VolleyballData.Domain
{
    /// <summary>
    /// An entity that the name search (<c>?q=</c>) can match: it keeps its name in the normalized form
    /// (<c>NameNormalizer.Normalize</c>) in a column with a trigram index.
    /// </summary>
    internal interface ISearchableName
    {
        string NormalizedName { get; }
    }
}
