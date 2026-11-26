using FantasyVolleyballLeague.Importer.Import;
using FantasyVolleyballLeague.Importer.Records;
using FantasyVolleyballLeague.Shared.Constants;
using Microsoft.Extensions.Options;

namespace FantasyVolleyballLeague.Importer.Tests.Import
{
    public sealed class RawDataReaderTests
    {
        private static readonly string DataRoot = Path.Combine(AppContext.BaseDirectory, "Fixtures", "RawData");

        [Fact]
        public void GetFiles_Should_ReturnTheFilesOfAllSeasonsInOrder()
        {
            var files = CreateReader().GetFiles("matches", "TestLeague");

            files.Select(file => Path.GetFileName(file)).ShouldBe(["1.json", "2.json", "3.json"]);
        }

        [Fact]
        public void GetFiles_Should_SkipTheManifestAndNonJsonFiles()
        {
            var files = CreateReader().GetFiles("matches", "TestLeague");

            var names = files.Select(file => Path.GetFileName(file)).ToList();
            names.ShouldNotContain("_manifest.json");
            names.ShouldNotContain("notes.txt");
        }

        [Fact]
        public void GetFiles_Should_ReturnNothing_When_TheLeagueFolderDoesNotExist()
        {
            var files = CreateReader().GetFiles("matches", "NoSuchLeague");

            files.ShouldBeEmpty();
        }

        [Fact]
        public async Task ReadAsync_Should_ReadTheRecord_IgnoringTheCasingOfPropertyNames()
        {
            var record = await RawDataReader.ReadAsync<Sample>(FilePath("valid.json"), sample => sample.SchemaVersion, TestContext.Current.CancellationToken);

            record.ShouldBe(new Sample(1, "Alpha"));
        }

        [Fact]
        public async Task ReadAsync_Should_Throw_When_TheSchemaVersionIsNotSupported()
        {
            var exception = await Should.ThrowAsync<InvalidDataException>(
                () => RawDataReader.ReadAsync<Sample>(FilePath("wrong-version.json"), sample => sample.SchemaVersion, TestContext.Current.CancellationToken));

            exception.Message.ShouldContain("Unsupported schema version 2");
        }

        [Fact]
        public async Task ReadAsync_Should_Throw_When_TheFileIsEmpty()
        {
            var exception = await Should.ThrowAsync<InvalidDataException>(
                () => RawDataReader.ReadAsync<Sample>(FilePath("empty.json"), sample => sample.SchemaVersion, TestContext.Current.CancellationToken));

            exception.Message.ShouldContain("empty");
        }

        [Fact]
        public async Task ReadAsync_Should_ReadTheStaffOfARosterFile()
        {
            var roster = await RawDataReader.ReadAsync<RosterFile>(FilePath("roster-with-staff.json"), file => file.SchemaVersion, TestContext.Current.CancellationToken);

            roster.Staff.ShouldBe(
            [
                new StaffMemberFile("Jan Kowalski", StaffRoleNames.HeadCoach),
                new StaffMemberFile("Anna Nowak", StaffRoleNames.President),
            ]);
        }

        [Fact]
        public async Task ReadAsync_Should_ReadNoStaff_When_TheRosterFileHasNone()
        {
            var roster = await RawDataReader.ReadAsync<RosterFile>(FilePath("valid.json"), file => file.SchemaVersion, TestContext.Current.CancellationToken);

            roster.Staff.ShouldBeEmpty();
        }

        private static RawDataReader CreateReader()
        {
            return new RawDataReader(Options.Create(new StorageOptions { DataRoot = DataRoot }));
        }

        private static string FilePath(string fileName)
        {
            return Path.Combine(DataRoot, "files", fileName);
        }

        private sealed record Sample(int SchemaVersion, string Name);
    }
}
