using System.Net;
using StockSync.Api.IntegrationTests.Infrastructure;
using StockSync.Application.Features.Auth;

namespace StockSync.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class RefreshTokenTests(ApiFactory factory) : IDisposable
{
    public void Dispose() => factory.Clock.Reset();

    [Fact]
    public async Task Refresh_rotates_the_token()
    {
        var seller = await Seller.RegisterAsync(factory);

        var rotated = await Seller.RefreshAsync(factory, seller.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, rotated.StatusCode);
        var next = Seller.ReadRefreshCookie(rotated);
        Assert.NotNull(next);
        Assert.NotEqual(seller.RefreshToken, next);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Seller.RefreshAsync(factory, seller.RefreshToken)).StatusCode);
    }

    [Fact]
    public async Task Reusing_a_just_rotated_token_fails_without_ending_the_new_session()
    {
        var seller = await Seller.RegisterAsync(factory);
        var next = Seller.ReadRefreshCookie(await Seller.RefreshAsync(factory, seller.RefreshToken))!;

        var replay = await Seller.RefreshAsync(factory, seller.RefreshToken);
        var continued = await Seller.RefreshAsync(factory, next);

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal("Auth.RefreshTokenRotated", await replay.ProblemCodeAsync());
        Assert.False(replay.Headers.Contains("Set-Cookie"));
        Assert.Equal(HttpStatusCode.OK, continued.StatusCode);
    }

    [Fact]
    public async Task Reusing_an_old_token_after_the_grace_period_revokes_every_session()
    {
        var seller = await Seller.RegisterAsync(factory);
        var next = Seller.ReadRefreshCookie(await Seller.RefreshAsync(factory, seller.RefreshToken))!;

        factory.Clock.Advance(RefreshSession.ConcurrentRotationGracePeriod + TimeSpan.FromSeconds(1));
        var replay = await Seller.RefreshAsync(factory, seller.RefreshToken);
        var afterReplay = await Seller.RefreshAsync(factory, next);

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, afterReplay.StatusCode);
    }

    [Fact]
    public async Task Parallel_refreshes_with_one_token_never_fork_the_session()
    {
        var seller = await Seller.RegisterAsync(factory);

        var responses = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => Seller.RefreshAsync(factory, seller.RefreshToken)));

        var winners = responses.Where(r => r.StatusCode == HttpStatusCode.OK).ToList();
        Assert.Single(winners);
        Assert.All(responses.Except(winners), r =>
        {
            Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
            Assert.False(r.Headers.Contains("Set-Cookie"));
        });
    }

    [Fact]
    public async Task Sessions_end_after_their_absolute_lifetime_even_when_refreshed()
    {
        var seller = await Seller.RegisterAsync(factory);
        var token = seller.RefreshToken;
        var statuses = new List<HttpStatusCode>();

        for (var step = 0; step < 5; step++)
        {
            factory.Clock.Advance(TimeSpan.FromDays(6));
            var response = await Seller.RefreshAsync(factory, token);
            statuses.Add(response.StatusCode);
            token = Seller.ReadRefreshCookie(response) ?? token;
        }

        Assert.Equal(
            [HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Unauthorized],
            statuses);
    }

    [Fact]
    public async Task Missing_or_unknown_tokens_are_rejected()
    {
        var unknown = await Seller.RefreshAsync(factory, "not-a-real-token");

        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal("Auth.InvalidRefreshToken", await unknown.ProblemCodeAsync());
    }
}
