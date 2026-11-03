using System.Net;
using System.Net.Http.Json;
using StockSync.Api.Endpoints;
using StockSync.Api.IntegrationTests.Infrastructure;
using StockSync.Application.Features.Auth;

namespace StockSync.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class AuthTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Register_returns_access_token_and_sets_hardened_refresh_cookie()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/register",
            new { storeName = "Kaya Aksesuar", displayName = "Mehmet Kaya", email = $"Mehmet.{Guid.NewGuid():N}@Example.com", password = Seller.Password },
            Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.ReadAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal("Kaya Aksesuar", body.User.StoreName);
        Assert.Equal("Owner", body.User.Role);
        Assert.Equal(body.User.Email.ToLowerInvariant(), body.User.Email);

        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_rejects_weak_password_with_validation_problem()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/register",
            new { storeName = "Store", displayName = "Owner", email = "weak@test.dev", password = "short" },
            Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Password", await response.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Register_with_taken_email_returns_conflict()
    {
        var email = $"taken-{Guid.NewGuid():N}@test.dev";
        await Seller.RegisterAsync(factory, email);

        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/register",
            new { storeName = "Other", displayName = "Other", email = email.ToUpperInvariant(), password = Seller.Password },
            Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Auth.EmailTaken", await response.ProblemCodeAsync());
    }

    [Fact]
    public async Task Login_succeeds_with_correct_password_and_fails_otherwise()
    {
        var seller = await Seller.RegisterAsync(factory);
        var client = factory.CreateClient();

        var ok = await client.PostAsJsonAsync("/api/auth/login", new { email = seller.Auth.User.Email, password = Seller.Password }, Ct);
        var wrong = await client.PostAsJsonAsync("/api/auth/login", new { email = seller.Auth.User.Email, password = "Wrong-passw0rd" }, Ct);
        var unknown = await client.PostAsJsonAsync("/api/auth/login", new { email = "nobody@test.dev", password = Seller.Password }, Ct);

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal(await wrong.ProblemCodeAsync(), await unknown.ProblemCodeAsync());
    }

    [Fact]
    public async Task Protected_endpoints_require_a_token()
    {
        var response = await factory.CreateClient().GetAsync("/api/products", Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_the_signed_in_user()
    {
        var seller = await Seller.RegisterAsync(factory);

        var me = await seller.GetAsync<UserDto>("/api/auth/me");

        Assert.Equal(seller.Auth.User.Id, me.Id);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token_and_clears_the_cookie()
    {
        var seller = await Seller.RegisterAsync(factory);

        using var logout = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logout.Headers.Add("Cookie", $"{RefreshTokenCookie.Name}={seller.RefreshToken}");
        var response = await factory.CreateClient().SendAsync(logout, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(Seller.ReadRefreshCookie(response));
        Assert.Equal(HttpStatusCode.Unauthorized, (await Seller.RefreshAsync(factory, seller.RefreshToken)).StatusCode);
    }
}
