namespace FantasyVolleyballLeague.Worker
{
    public static class PlaywrightUtilities
    {
        public static async Task TryClickDenyCookiesAsync(PlaywrightSession session)
        {
            var page = session.Context.Pages[0] ?? await session.Context.NewPageAsync();
            var button = page.Locator("button.button-deny");

            if (await button.IsVisibleAsync())
            {
                await button.ClickAsync();
                Console.WriteLine("Clicked deny cookies successfully.");
            }
            else
            {
                Console.WriteLine("No deny cookies button found.");
            }
        }
    }
}
