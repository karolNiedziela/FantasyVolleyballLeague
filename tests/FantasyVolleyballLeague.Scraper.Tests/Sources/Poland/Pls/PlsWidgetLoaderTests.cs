using FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Scraper.Tests.Sources.Poland.Pls
{
    public sealed class PlsWidgetLoaderTests
    {
        [Theory]
        [InlineData("Attention Required! | Cloudflare")]
        [InlineData("Just a moment...")]
        [InlineData("attention required")]
        public void IsBlockPageTitleDetectsCloudflarePages(string title)
        {
            Assert.True(PlsWidgetLoader.IsBlockPageTitle(title));
        }

        [Theory]
        [InlineData("")]
        [InlineData("Team stats")]
        public void IsBlockPageTitleIgnoresOtherPages(string title)
        {
            Assert.False(PlsWidgetLoader.IsBlockPageTitle(title));
        }

        [Fact]
        public void IsRetryableCoversPlaywrightTimeouts()
        {
            Assert.True(PlsWidgetLoader.IsRetryable(new TimeoutException("Timeout 15000ms exceeded.")));
            Assert.True(PlsWidgetLoader.IsRetryable(new PlaywrightException("Page crashed")));
            Assert.True(PlsWidgetLoader.IsRetryable(new SourceBlockedException("blocked")));
            Assert.False(PlsWidgetLoader.IsRetryable(new FormatException()));
        }
    }
}
