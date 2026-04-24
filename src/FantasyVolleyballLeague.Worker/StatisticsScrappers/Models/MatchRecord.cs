using FantasyVolleyballLeague.Worker.StatisticsScrapper.Models;

namespace FantasyVolleyballLeague.Worker.StatisticsScrappers.Models
{
    public sealed record MatchRecord(
        int MatchId,
        DateTime Date,
        string? MatchNumber,
        string? Mvp,
        int? Attendance,
        string? FirstReferee,
        string? SecondReferee,
        string? Commissioner,
        string? VenueName,
        string? VenueCity,
        IReadOnlyList<SetScore> Sets,
        MatchTeamStatistics FirstTeam,
        MatchTeamStatistics SecondTeam);
}
