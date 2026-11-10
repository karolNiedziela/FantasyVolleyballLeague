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
            PlsWidgetLoader.IsBlockPageTitle(title).ShouldBeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("Team stats")]
        public void IsBlockPageTitleIgnoresOtherPages(string title)
        {
            PlsWidgetLoader.IsBlockPageTitle(title).ShouldBeFalse();
        }

        [Fact]
        public void IsRetryableCoversPlaywrightTimeouts()
        {
            PlsWidgetLoader.IsRetryable(new TimeoutException("Timeout 15000ms exceeded.")).ShouldBeTrue();
            PlsWidgetLoader.IsRetryable(new PlaywrightException("Page crashed")).ShouldBeTrue();
            PlsWidgetLoader.IsRetryable(new SourceBlockedException("blocked")).ShouldBeTrue();
            PlsWidgetLoader.IsRetryable(new FormatException()).ShouldBeFalse();
        }
    }
}
