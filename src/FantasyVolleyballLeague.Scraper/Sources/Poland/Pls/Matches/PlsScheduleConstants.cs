namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    internal static class PlsScheduleConstants
    {
        internal static class Attributes
        {
            internal const string FilterListType = "data-filter-list-type";
            internal const string FilterValue = "data-filter-value";
            internal const string Phase = "data-phase";
            internal const string Round = "data-round";
            internal const string Term = "data-term";
            internal const string GameId = "data-game-id";
        }

        internal static class FilterListTypes
        {
            internal const string Phase = "phase";
        }

        internal static class CssClasses
        {
            internal const string FilterableContent = "filterable-content";
            internal const string AjaxSyncedGames = "ajax-synced-games";
        }

        internal static class Polish
        {
            internal const string AllFilter = "Wszystkie";
            internal const string AllFilterValue = "all";
            internal const string RoundPrefix = "Runda ";
            internal const string NumberAbbreviation = "nr";
        }
    }
}
