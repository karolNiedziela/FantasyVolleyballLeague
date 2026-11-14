using FantasyVolleyballLeague.DataApi.Common.API.Paging;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Paging
{
    public sealed class PagedResponseTests
    {
        [Fact]
        public void ConstructorKeepsTheValues()
        {
            var page = new PagedResponse<int>([1, 2], offset: 50, limit: 25, totalCount: 52);

            page.Items.ShouldBe([1, 2]);
            page.Offset.ShouldBe(50);
            page.Limit.ShouldBe(25);
            page.TotalCount.ShouldBe(52);
        }

        [Fact]
        public void ConstructorAcceptsAnEmptyPageBeyondTheEnd()
        {
            var page = new PagedResponse<int>([], offset: 500, limit: 25, totalCount: 60);

            page.Items.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(-1, 25, 10)]
        [InlineData(0, 0, 10)]
        [InlineData(0, 25, -1)]
        public void ConstructorRejectsOutOfRangeValues(int offset, int limit, int totalCount)
        {
            Should.Throw<ArgumentOutOfRangeException>(() => new PagedResponse<int>([], offset, limit, totalCount));
        }

        [Fact]
        public void ConstructorRejectsMoreItemsThanTheLimit()
        {
            var exception = Should.Throw<ArgumentOutOfRangeException>(() => new PagedResponse<int>([1, 2, 3], offset: 0, limit: 2, totalCount: 3));

            exception.ParamName.ShouldBe("items");
        }

        [Fact]
        public void ConstructorRejectsNullItems()
        {
            Should.Throw<ArgumentNullException>(() => new PagedResponse<int>(null!, offset: 0, limit: 25, totalCount: 0));
        }
    }
}
