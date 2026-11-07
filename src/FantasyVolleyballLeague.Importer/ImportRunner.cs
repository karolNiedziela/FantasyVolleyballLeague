using FantasyVolleyballLeague.Importer.Import;

namespace FantasyVolleyballLeague.Importer
{
    internal sealed class ImportRunner
    {
        private readonly IEnumerable<IImportStep> _steps;
        private readonly ImportReport _report;
        private readonly RawDataReader _reader;

        public ImportRunner(IEnumerable<IImportStep> steps, ImportReport report, RawDataReader reader)
        {
            _steps = steps;
            _report = report;
            _reader = reader;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Data root: {_reader.DataRoot}");

            var orderedSteps = _steps.OrderBy(m => m.Order);
            foreach (var step in orderedSteps)
            {
                Console.WriteLine($"Running step: {step.Name}");

                await step.ExecuteAsync(cancellationToken);

                Console.WriteLine($"Finished step: {step.Name}");
            }

            var reportPath = await _report.WriteAsync(_reader.DataRoot, cancellationToken);
            Console.WriteLine(
                $"Rosters: {_report.RostersImported}; matches inserted: {_report.MatchesInserted}, updated: {_report.MatchesUpdated}, " +
                $"skipped: {_report.MatchesSkipped}; players by roster: {_report.PlayersResolvedByRoster}, by season: {_report.PlayersResolvedBySeason}, " +
                $"unresolved players: {_report.UnresolvedPlayers.Count}, matches without MVP: {_report.MatchesWithoutMvp}, " +
                $"file errors: {_report.FileErrors.Count}, warnings: {_report.Warnings.Count}");
            Console.WriteLine($"Report: {reportPath}");
        }
    }
}
