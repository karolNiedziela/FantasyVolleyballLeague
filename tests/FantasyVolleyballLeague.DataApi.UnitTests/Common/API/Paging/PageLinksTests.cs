using FantasyVolleyballLeague.DataApi.Common.API.Paging;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Paging
{
    public sealed class PageLinksTests
    {
        private const string Path = "/api/v1/leagues";

        [Fact]
        public void Create_Should_OmitPrev_OnTheFirstPage()
        {
            var links = PageLinks.Create(Path, 0, 25, 100);

            links.ShouldBe(new PageLinks(
                "/api/v1/leagues?offset=0&limit=25",
                "/api/v1/leagues?offset=0&limit=25",
                null,
                "/api/v1/leagues?offset=25&limit=25",
                "/api/v1/leagues?offset=75&limit=25"));
        }

        [Fact]
        public void Create_Should_IncludeAllRelations_OnAMiddlePage()
        {
            var links = PageLinks.Create(Path, 25, 25, 100);

            links.ShouldBe(new PageLinks(
                "/api/v1/leagues?offset=25&limit=25",
                "/api/v1/leagues?offset=0&limit=25",
                "/api/v1/leagues?offset=0&limit=25",
                "/api/v1/leagues?offset=50&limit=25",
                "/api/v1/leagues?offset=75&limit=25"));
        }

        [Fact]
        public void Create_Should_OmitNext_OnTheLastPage()
        {
            var links = PageLinks.Create(Path, 75, 25, 100);

            links.ShouldBe(new PageLinks(
                "/api/v1/leagues?offset=75&limit=25",
                "/api/v1/leagues?offset=0&limit=25",
                "/api/v1/leagues?offset=50&limit=25",
                null,
                "/api/v1/leagues?offset=75&limit=25"));
        }

        [Fact]
        public void Create_Should_HandleAPartialLastPage()
        {
            var links = PageLinks.Create(Path, 0, 25, 60);

            links.Last.ShouldBe("/api/v1/leagues?offset=50&limit=25");
        }

        [Fact]
        public void Create_Should_ClampPrevAtZero_When_TheOffsetIsNotAlignedToTheLimit()
        {
            var links = PageLinks.Create(Path, 10, 25, 100);

            links.Prev.ShouldBe("/api/v1/leagues?offset=0&limit=25");
            links.Next.ShouldBe("/api/v1/leagues?offset=35&limit=25");
        }

        [Fact]
        public void Create_Should_PointFirstAndLastAtTheSamePage_When_ThereAreNoRows()
        {
            var links = PageLinks.Create(Path, 0, 25, 0);

            links.ShouldBe(new PageLinks(
                "/api/v1/leagues?offset=0&limit=25",
                "/api/v1/leagues?offset=0&limit=25",
                null,
                null,
                "/api/v1/leagues?offset=0&limit=25"));
        }

        [Fact]
        public void Create_Should_PointPrevAtTheLastPage_When_TheOffsetIsBeyondTheEnd()
        {
            var links = PageLinks.Create(Path, 500, 25, 60);

            links.ShouldBe(new PageLinks(
                "/api/v1/leagues?offset=500&limit=25",
                "/api/v1/leagues?offset=0&limit=25",
                "/api/v1/leagues?offset=50&limit=25",
                null,
                "/api/v1/leagues?offset=50&limit=25"));
        }

        [Fact]
        public void Create_Should_WriteTheEffectiveOffsetAndLimitInSelf_When_TheUrlHasNone()
        {
            var links = PageLinks.Create(Path, 0, 25, 10);

            links.Self.ShouldBe("/api/v1/leagues?offset=0&limit=25");
        }

        [Fact]
        public void Create_Should_ReplaceThePagingParametersInSelfIgnoringCase_And_KeepTheOthers()
        {
            var links = PageLinks.Create("/api/v1/leagues?sort=-name&Offset=5&LIMIT=7&gender=1", 5, 7, 20);

            links.Self.ShouldBe("/api/v1/leagues?sort=-name&gender=1&offset=5&limit=7");
        }

        [Fact]
        public void Create_Should_KeepTheOtherQueryParametersInFirstAndNext()
        {
            var links = PageLinks.Create("/api/v1/leagues?sort=-name&Offset=5&LIMIT=7&gender=1", 5, 7, 20);

            links.First.ShouldBe("/api/v1/leagues?sort=-name&gender=1&offset=0&limit=7");
            links.Next.ShouldBe("/api/v1/leagues?sort=-name&gender=1&offset=12&limit=7");
        }

        [Fact]
        public void Create_Should_EncodeQueryValues()
        {
            var links = PageLinks.Create("/api/v1/leagues?q=aluron%20cmc", 0, 25, 10);

            links.First.ShouldBe("/api/v1/leagues?q=aluron%20cmc&offset=0&limit=25");
        }

        [Fact]
        public void Create_Should_KeepTheSchemeHostAndPortOfAnAbsoluteUrl()
        {
            var links = PageLinks.Create("https://api.example.test:8443/api/v1/leagues?sort=name", 0, 25, 60);

            links.ShouldBe(new PageLinks(
                "https://api.example.test:8443/api/v1/leagues?sort=name&offset=0&limit=25",
                "https://api.example.test:8443/api/v1/leagues?sort=name&offset=0&limit=25",
                null,
                "https://api.example.test:8443/api/v1/leagues?sort=name&offset=25&limit=25",
                "https://api.example.test:8443/api/v1/leagues?sort=name&offset=50&limit=25"));
        }

        [Theory]
        [InlineData(-1, 25, 10)]
        [InlineData(0, 0, 10)]
        [InlineData(0, 25, -1)]
        public void Create_Should_RejectOutOfRangeValues(int offset, int limit, int totalCount)
        {
            Should.Throw<ArgumentOutOfRangeException>(() => PageLinks.Create(Path, offset, limit, totalCount));
        }
    }
}
