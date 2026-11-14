using FantasyVolleyballLeague.DataApi.Common.API.Paging;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Paging
{
    public sealed class PageLinksTests
    {
        private const string Path = "/api/v1/leagues";

        [Fact]
        public void CreateOmitsPrevOnTheFirstPage()
        {
            var links = PageLinks.Create(Path, 0, 25, 100);

            links.ShouldBe(new PageLinks(Page(0, 25), Page(0, 25), null, Page(25, 25), Page(75, 25)));
        }

        [Fact]
        public void CreateIncludesAllRelationsOnAMiddlePage()
        {
            var links = PageLinks.Create(Path, 25, 25, 100);

            links.ShouldBe(new PageLinks(Page(25, 25), Page(0, 25), Page(0, 25), Page(50, 25), Page(75, 25)));
        }

        [Fact]
        public void CreateOmitsNextOnTheLastPage()
        {
            var links = PageLinks.Create(Path, 75, 25, 100);

            links.ShouldBe(new PageLinks(Page(75, 25), Page(0, 25), Page(50, 25), null, Page(75, 25)));
        }

        [Fact]
        public void CreateHandlesAPartialLastPage()
        {
            var links = PageLinks.Create(Path, 0, 25, 60);

            links.Last.ShouldBe(Page(50, 25));
        }

        [Fact]
        public void CreateClampsPrevAtZeroWhenTheOffsetIsNotAlignedToTheLimit()
        {
            var links = PageLinks.Create(Path, 10, 25, 100);

            links.Prev.ShouldBe(Page(0, 25));
            links.Next.ShouldBe(Page(35, 25));
        }

        [Fact]
        public void CreatePointsFirstAndLastAtTheSamePageWhenThereAreNoRows()
        {
            var links = PageLinks.Create(Path, 0, 25, 0);

            links.ShouldBe(new PageLinks(Page(0, 25), Page(0, 25), null, null, Page(0, 25)));
        }

        [Fact]
        public void CreatePointsPrevAtTheLastPageWhenTheOffsetIsBeyondTheEnd()
        {
            var links = PageLinks.Create(Path, 500, 25, 60);

            links.ShouldBe(new PageLinks(Page(500, 25), Page(0, 25), Page(50, 25), null, Page(50, 25)));
        }

        [Fact]
        public void CreateWritesTheEffectiveOffsetAndLimitInSelfWhenTheUrlHasNone()
        {
            var links = PageLinks.Create(Path, 0, 25, 10);

            links.Self.ShouldBe(Page(0, 25));
        }

        [Fact]
        public void CreateKeepsOtherQueryParametersAndReplacesPagingOnes()
        {
            var links = PageLinks.Create($"{Path}?sort=-name&Offset=5&LIMIT=7&gender=1", 5, 7, 20);

            links.Self.ShouldBe($"{Path}?sort=-name&gender=1&offset=5&limit=7");
            links.First.ShouldBe($"{Path}?sort=-name&gender=1&offset=0&limit=7");
            links.Next.ShouldBe($"{Path}?sort=-name&gender=1&offset=12&limit=7");
            links.Self.ShouldNotContain("Offset=", Case.Sensitive);
            links.Self.ShouldNotContain("LIMIT=", Case.Sensitive);
        }

        [Fact]
        public void CreateEncodesQueryValues()
        {
            var links = PageLinks.Create($"{Path}?q=aluron%20cmc", 0, 25, 10);

            links.First.ShouldBe($"{Path}?q=aluron%20cmc&offset=0&limit=25");
        }

        [Fact]
        public void CreateKeepsTheSchemeHostAndPortOfAnAbsoluteUrl()
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
        public void CreateRejectsOutOfRangeValues(int offset, int limit, int totalCount)
        {
            Should.Throw<ArgumentOutOfRangeException>(() => PageLinks.Create(Path, offset, limit, totalCount));
        }

        private static string Page(int offset, int limit)
        {
            return $"{Path}?offset={offset}&limit={limit}";
        }
    }
}
