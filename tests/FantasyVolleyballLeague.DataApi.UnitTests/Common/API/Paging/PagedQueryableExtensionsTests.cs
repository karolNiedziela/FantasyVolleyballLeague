using FantasyVolleyballLeague.DataApi.Common.API.Paging;

namespace FantasyVolleyballLeague.DataApi.UnitTests.Common.API.Paging
{
    public sealed class PagedQueryableExtensionsTests
    {
        [Theory]
        [InlineData(-1, 10)]
        [InlineData(0, 0)]
        public async Task ToPagedResponseAsyncRejectsOutOfRangeValues(int offset, int limit)
        {
            await Should.ThrowAsync<ArgumentOutOfRangeException>(
                () => Array.Empty<int>().AsQueryable().ToPagedResponseAsync(offset, limit));
        }
    }
}
