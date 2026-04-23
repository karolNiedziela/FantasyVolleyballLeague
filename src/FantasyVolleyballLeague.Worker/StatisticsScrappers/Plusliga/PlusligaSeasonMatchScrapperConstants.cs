namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Plusliga
{
    internal static class PlusligaSeasonMatchScrapperConstants
    {
        internal static class Attributes
        {
            internal const string FilterListType = "data-filter-list-type";
            internal const string FilterValue = "data-filter-value";
            internal const string FilterType = "data-filter-type";
            internal const string Phase = "data-phase";
            internal const string Round = "data-round";
            internal const string Term = "data-term";
            internal const string GameId = "data-game-id";
            internal const string Style = "style";
        }

        internal static class FilterListTypes
        {
            internal const string Phase = "phase";
            internal const string Round = "round";
            internal const string Term = "term";
        }

        internal static class CssClasses
        {
            internal const string FilterableContent = "filterable-content";
            internal const string AjaxSyncedGames = "ajax-synced-games";
        }

        internal static class DisplayStyles
        {
            internal const string DisplayNone = "display:none";
            internal const string DisplayNoneWithSpace = "display: none";
        }

        internal static class Polish
        {
            internal const string AllFilter = "Wszystkie";
            internal const string AllFilterValue = "all";
            internal const string RoundPrefix = "Runda ";
            internal const string TermNumberPrefix = "nr: ";
            internal const string TermNumberPrefixShort = "nr:";
        }

        internal const string DefaultStageName = "Stage";
    }
}
