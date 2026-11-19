using System.Net;

namespace FantasyVolleyballLeague.DataApi.IntegrationTests.Fixtures
{
    internal static class HttpResponseAssertions
    {
        public static async Task ShouldHaveStatusCode(this HttpResponseMessage response, HttpStatusCode expected)
        {
            if (response.StatusCode == expected)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            response.StatusCode.ShouldBe(expected, $"The response body was: {body}");
        }

        public static void ShouldHaveMediaType(this HttpResponseMessage response, string expected)
        {
            response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe(expected);
        }
    }
}
