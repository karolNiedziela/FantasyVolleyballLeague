namespace FantasyVolleyballLeague.Api.Domain
{
    internal sealed class Match : Entity
    {
        private readonly List<SetScore> _sets = [];
        private readonly List<PlayerMatchStatistics> _playerStatistics = [];

        /// <summary>
        /// Match id on the league website.
        /// </summary>
        public int? ExternalId { get; private set; }

        /// <summary>
        /// UTC. For scheduled matches the time may not be confirmed yet (see <see cref="IsTimeConfirmed"/>).
        /// </summary>
        public DateTimeOffset? Date { get; private set; }

        public bool IsTimeConfirmed { get; private set; }

        public int StatusId { get; private set; }

        public MatchStatus Status { get; private set; } = default!;

        public int StatisticsLevelId { get; private set; }

        public StatisticsLevel StatisticsLevel { get; private set; } = default!;

        public string? MatchNumber { get; private set; }

        public int? Attendance { get; private set; }

        public Venue Venue { get; private set; } = new(null, null);

        public MatchOfficials Officials { get; private set; } = new(null, null, null);

        /// <summary>
        /// Scrape time of the raw file this match was imported from; newer files update the match.
        /// </summary>
        public DateTimeOffset SourceScrapedAt { get; private set; }

        public Guid LeagueId { get; private set; }

        public League League { get; private set; } = default!;

        public Guid? MvpId { get; private set; }

        public Player? Mvp { get; private set; } = default!;

        public Guid HomeRosterId { get; private set; }

        public Roster HomeRoster { get; private set; } = default!;

        public Guid AwayRosterId { get; private set; }

        public Roster AwayRoster { get; private set; } = default!;

        public Guid RoundId { get; private set; }

        public Round Round { get; private set; } = default!;

        public IReadOnlyCollection<SetScore> Sets => _sets.AsReadOnly();

        public IReadOnlyCollection<PlayerMatchStatistics> PlayerStatistics => _playerStatistics.AsReadOnly();

        private Match() { }

        public Match(int? externalId, Guid leagueId, Guid roundId, Guid homeRosterId, Guid awayRosterId, DateTimeOffset? date, bool isTimeConfirmed)
        {
            ExternalId = externalId;
            LeagueId = leagueId;
            StatusId = MatchStatus.Scheduled.Id;
            StatisticsLevelId = StatisticsLevel.None.Id;
            Reschedule(roundId, homeRosterId, awayRosterId, date, isTimeConfirmed);
        }

        public void Reschedule(Guid roundId, Guid homeRosterId, Guid awayRosterId, DateTimeOffset? date, bool isTimeConfirmed)
        {
            RoundId = roundId;
            HomeRosterId = homeRosterId;
            AwayRosterId = awayRosterId;
            Date = date;
            IsTimeConfirmed = isTimeConfirmed;
        }

        public void UpdateDetails(int statusId, string? matchNumber, int? attendance, Venue venue, MatchOfficials officials)
        {
            StatusId = statusId;
            MatchNumber = matchNumber;
            Attendance = attendance;
            Venue = venue;
            Officials = officials;
        }

        public void ReplaceSets(IEnumerable<(int HomeScore, int AwayScore)> sets)
        {
            _sets.Clear();

            var setNumber = 1;
            foreach (var (homeScore, awayScore) in sets)
            {
                _sets.Add(new SetScore(setNumber++, homeScore, awayScore, Id));
            }
        }

        public void ReplacePlayerStatistics(int statisticsLevelId, IEnumerable<PlayerMatchStatistics> playerStatistics)
        {
            StatisticsLevelId = statisticsLevelId;
            _playerStatistics.Clear();
            _playerStatistics.AddRange(playerStatistics);
        }

        public void AssignMvp(Guid? playerId)
        {
            MvpId = playerId;
        }

        public void MarkSynced(DateTimeOffset scrapedAt)
        {
            SourceScrapedAt = scrapedAt;
        }
    }
}
