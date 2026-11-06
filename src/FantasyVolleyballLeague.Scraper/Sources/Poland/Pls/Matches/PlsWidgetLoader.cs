using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace FantasyVolleyballLeague.Scraper.Sources.Poland.Pls.Matches
{
    /// <summary>
    /// Loads the rendered HTML of a volleystation statistics widget. The widget host sits behind Cloudflare
    /// and starts blocking clients that send too many requests, so loads are spaced out, a block page is
    /// detected immediately (instead of waiting for the widget until the timeout) and failed loads are
    /// retried with a growing delay.
    /// </summary>
    internal sealed class PlsWidgetLoader
    {
        private const string WidgetSelector = ".team-stats-widget";

        private static readonly int[] BlockedStatusCodes = [403, 429, 503];

        private static readonly string[] BlockPageTitles = ["Attention Required", "Just a moment"];

        private readonly PlaywrightOptions _options;
        private readonly Lock _throttleLock = new();
        private DateTime _nextSlotUtc = DateTime.MinValue;

        public PlsWidgetLoader(IOptions<PlaywrightOptions> options)
        {
            _options = options.Value;
        }

        public async Task<string> LoadHtmlAsync(PlaywrightSession session, string url)
        {
            var retries = 0;
            while (true)
            {
                await WaitForTurnAsync();

                try
                {
                    return await LoadOnceAsync(session, url);
                }
                catch (Exception ex) when (retries < _options.WidgetMaxRetries && IsRetryable(ex))
                {
                    var delayMs = _options.WidgetRetryDelayMs << retries;
                    retries++;
                    Console.WriteLine($"Widget load failed ({FirstLine(ex.Message)}). Retry {retries}/{_options.WidgetMaxRetries} in {delayMs} ms: {url}");
                    await Task.Delay(delayMs);
                }
            }
        }

        private async Task<string> LoadOnceAsync(PlaywrightSession session, string url)
        {
            var page = await session.Context.NewPageAsync();
            try
            {
                var response = await page.GotoAsync(url, new PageGotoOptions { Timeout = _options.WidgetTimeoutMs });
                if (response is not null && BlockedStatusCodes.Contains(response.Status))
                {
                    throw BlockedException(url, $"HTTP {response.Status}");
                }

                try
                {
                    await page.WaitForSelectorAsync(WidgetSelector, new PageWaitForSelectorOptions { Timeout = _options.WidgetTimeoutMs });
                }
                catch (Exception ex) when (IsRetryable(ex))
                {
                    var title = await page.TitleAsync();
                    if (IsBlockPageTitle(title))
                    {
                        throw BlockedException(url, $"page title '{title}'");
                    }

                    throw;
                }

                return await page.ContentAsync();
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        /// <summary>
        /// Reserves the next start slot, so two widget loads never start closer than
        /// <see cref="PlaywrightOptions.WidgetMinIntervalMs"/> apart, whatever the number of parallel tasks.
        /// </summary>
        private async Task WaitForTurnAsync()
        {
            TimeSpan wait;
            lock (_throttleLock)
            {
                var now = DateTime.UtcNow;
                var slot = _nextSlotUtc > now ? _nextSlotUtc : now;
                _nextSlotUtc = slot.AddMilliseconds(_options.WidgetMinIntervalMs);
                wait = slot - now;
            }

            if (wait > TimeSpan.Zero)
            {
                await Task.Delay(wait);
            }
        }

        internal static bool IsRetryable(Exception exception)
        {
            return exception is PlaywrightException or TimeoutException or InvalidOperationException or SourceBlockedException;
        }

        internal static bool IsBlockPageTitle(string title)
        {
            return BlockPageTitles.Any(blockTitle => title.Contains(blockTitle, StringComparison.OrdinalIgnoreCase));
        }

        private static SourceBlockedException BlockedException(string url, string reason)
        {
            return new SourceBlockedException($"Widget host blocked the request ({reason}), probably Cloudflare rate limiting: {url}");
        }

        private static string FirstLine(string message)
        {
            return message.Split('\n')[0].Trim();
        }
    }
}
