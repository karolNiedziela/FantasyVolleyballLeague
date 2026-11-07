using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Scraper
{
    internal sealed class PlaywrightFactory
    {
        private readonly PlaywrightOptions _options;

        public PlaywrightFactory(IOptions<PlaywrightOptions> options)
        {
            _options = options.Value;
        }

        public async Task<PlaywrightSession> SetupBrowserContextAsync()
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = _options.Headless });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                Locale = "pl-PL"
            });
            await context.AddInitScriptAsync(@"Object.defineProperty(navigator, 'webdriver', { get: () => undefined });");

            return new PlaywrightSession(playwright, browser, context);
        }
    }
}
