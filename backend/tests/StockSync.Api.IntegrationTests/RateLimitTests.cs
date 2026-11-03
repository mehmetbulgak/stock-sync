using System.Net;
using System.Net.Http.Json;
using StockSync.Api.IntegrationTests.Infrastructure;

namespace StockSync.Api.IntegrationTests;

public sealed class RateLimitedApiFactory : ApiFactory
{
    public const int Limit = 3;

    protected override int AuthPermitLimit => Limit;
}

public sealed class AccountThrottledApiFactory : ApiFactory
{
    public const int MaxFailures = 3;

    protected override int MaxLoginFailuresPerAccount => MaxFailures;
}

public class RateLimitTests(RateLimitedApiFactory factory) : IClassFixture<RateLimitedApiFactory>
{
    [Fact]
    public async Task Repeated_sign_in_attempts_are_throttled_without_blocking_session_refresh()
    {
        var seller = await Seller.RegisterAsync(factory);
        var client = factory.CreateClient();
        var statuses = new List<HttpStatusCode>();

        for (var attempt = 1; attempt <= RateLimitedApiFactory.Limit; attempt++)
        {
            var response = await client.PostAsJsonAsync(
                "/api/auth/login",
                new { email = "victim@test.dev", password = $"guess-{attempt}" },
                TestContext.Current.CancellationToken);
            statuses.Add(response.StatusCode);
        }

        Assert.All(statuses[..^1], s => Assert.Equal(HttpStatusCode.Unauthorized, s));
        Assert.Equal(HttpStatusCode.TooManyRequests, statuses[^1]);
        Assert.Equal(HttpStatusCode.OK, (await Seller.RefreshAsync(factory, seller.RefreshToken)).StatusCode);
    }
}

public class AccountThrottleTests(AccountThrottledApiFactory factory) : IClassFixture<AccountThrottledApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Failed_sign_ins_lock_only_the_targeted_account()
    {
        var victim = await Seller.RegisterAsync(factory);
        var bystander = await Seller.RegisterAsync(factory);
        var client = factory.CreateClient();

        for (var attempt = 1; attempt <= AccountThrottledApiFactory.MaxFailures; attempt++)
        {
            var guess = await client.PostAsJsonAsync("/api/auth/login", new { email = victim.Auth.User.Email, password = $"guess-{attempt}" }, Ct);
            Assert.Equal(HttpStatusCode.Unauthorized, guess.StatusCode);
        }

        var blocked = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = victim.Auth.User.Email.ToUpperInvariant(), password = Seller.Password },
            Ct);
        var unaffected = await client.PostAsJsonAsync("/api/auth/login", new { email = bystander.Auth.User.Email, password = Seller.Password }, Ct);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal("Auth.TooManyFailedSignIns", await blocked.ProblemCodeAsync());
        Assert.Equal(HttpStatusCode.OK, unaffected.StatusCode);
    }
}
