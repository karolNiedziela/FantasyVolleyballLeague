using System.Text.Json;
using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class ManualDataReaderTests
    {
        private static readonly ManualDataReader Reader =
            new(Path.Combine(AppContext.BaseDirectory, "Fixtures", "ManualData"));

        [Fact]
        public async Task ReadAsync_Should_ReadTheEntries_IgnoringTheCasingOfPropertyNames()
        {
            var entries = await Reader.ReadAsync<LeagueNameFile>(ManualDataReader.LeagueNamesFileName, TestContext.Current.CancellationToken);

            entries.ShouldBe([
                new LeagueNameFile("PlusLiga", 2007, "PlusLiga"),
                new LeagueNameFile("PlusLiga", 2026, "TAURON Liga"),
            ]);
        }

        [Fact]
        public async Task ReadAsync_Should_ReadTheClubGroups()
        {
            var groups = await Reader.ReadAsync<ClubGroupFile>(ManualDataReader.ClubsFileName, TestContext.Current.CancellationToken);

            groups.Count.ShouldBe(2);
            groups[1].ShouldSatisfyAllConditions(
                () => groups[1].Key.ShouldBe("stal-nysa"),
                () => groups[1].Ids.ShouldBe(["PLS1Liga:69", "PLS1Liga:90", "PlusLiga:68"]));
        }

        [Fact]
        public async Task ReadAsync_Should_ReadTheClubSplits()
        {
            var splits = await Reader.ReadAsync<ClubSplitFile>(ManualDataReader.ClubSplitsFileName, TestContext.Current.CancellationToken);

            splits.ShouldBe([new ClubSplitFile("PLS1Liga:57", 2026)]);
        }

        [Fact]
        public async Task ReadValidatedAsync_Should_ReturnTheEntries_When_TheFileIsValid()
        {
            var result = await Reader.ReadValidatedAsync<LeagueNameFile>(
                ManualDataReader.LeagueNamesFileName,
                _ => [],
                TestContext.Current.CancellationToken);

            result.ShouldSatisfyAllConditions(
                () => result.Entries.ShouldNotBeNull().Count.ShouldBe(2),
                () => result.Errors.ShouldBeEmpty());
        }

        [Fact]
        public async Task ReadValidatedAsync_Should_ReturnTheValidationErrorsWithoutEntries()
        {
            var result = await Reader.ReadValidatedAsync<LeagueNameFile>(
                ManualDataReader.LeagueNamesFileName,
                _ => ["Entry 1: unknown league 'Plusliga'."],
                TestContext.Current.CancellationToken);

            result.ShouldSatisfyAllConditions(
                () => result.Entries.ShouldBeNull(),
                () => result.Errors.ShouldBe(["Entry 1: unknown league 'Plusliga'."]));
        }

        [Fact]
        public async Task ReadValidatedAsync_Should_ReturnTheReadErrorWithoutEntries_When_TheFileDoesNotExist()
        {
            var result = await Reader.ReadValidatedAsync<LeagueNameFile>(
                "missing.json",
                _ => [],
                TestContext.Current.CancellationToken);

            result.ShouldSatisfyAllConditions(
                () => result.Entries.ShouldBeNull(),
                () => result.Errors.ShouldHaveSingleItem().ShouldContain("missing.json"));
        }

        [Fact]
        public async Task ReadAsync_Should_Throw_When_TheFileDoesNotExist()
        {
            await Should.ThrowAsync<FileNotFoundException>(
                () => Reader.ReadAsync<LeagueNameFile>("missing.json", TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ReadAsync_Should_Throw_When_TheFileIsMalformed()
        {
            var directory = Directory.CreateTempSubdirectory();
            try
            {
                await File.WriteAllTextAsync(
                    Path.Combine(directory.FullName, "malformed.json"),
                    """[{ "league": "PlusLiga", """,
                    TestContext.Current.CancellationToken);
                var reader = new ManualDataReader(directory.FullName);

                await Should.ThrowAsync<JsonException>(
                    () => reader.ReadAsync<LeagueNameFile>("malformed.json", TestContext.Current.CancellationToken));
            }
            finally
            {
                directory.Delete(recursive: true);
            }
        }
    }
}
