using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Scraper
{
    internal sealed class PlaywrightSession : IAsyncDisposable
    {
        public IPlaywright Playwright { get; }
        public IBrowser Browser { get; }
        public IBrowserContext Context { get; }

        public PlaywrightSession(IPlaywright playwright, IBrowser browser, IBrowserContext context)
        {
            Playwright = playwright;
            Browser = browser;
            Context = context;
        }

        public async Task<string> GetHtmlAsync(string url)
        {
            var page = await Context.NewPageAsync();
            try
            {
                await page.GotoAsync(url);
                return await page.ContentAsync();
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Context.CloseAsync();
            await Browser.CloseAsync();
            Playwright.Dispose();
        }
    }
}
