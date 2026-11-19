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
        public void IsRetryable_Should_ReturnTrue_ForATimeout()
        {
            PlsWidgetLoader.IsRetryable(new TimeoutException("Timeout 15000ms exceeded.")).ShouldBeTrue();
        }

        [Fact]
        public void IsRetryable_Should_ReturnTrue_ForAPlaywrightError()
        {
            PlsWidgetLoader.IsRetryable(new PlaywrightException("Page crashed")).ShouldBeTrue();
        }

        [Fact]
        public void IsRetryable_Should_ReturnTrue_WhenTheSourceBlockedTheRequest()
        {
            PlsWidgetLoader.IsRetryable(new SourceBlockedException("blocked")).ShouldBeTrue();
        }

        [Fact]
        public void IsRetryable_Should_ReturnFalse_ForOtherExceptions()
        {
            PlsWidgetLoader.IsRetryable(new FormatException()).ShouldBeFalse();
        }
    }
}
