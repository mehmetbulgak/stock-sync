using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StockSync.Api.IntegrationTests.Infrastructure;

namespace StockSync.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class SecurityTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Token_signed_with_another_key_is_rejected()
    {
        var seller = await Seller.RegisterAsync(factory);
        var forged = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "stocksync",
            Audience = "stocksync-web",
            Expires = DateTime.UtcNow.AddMinutes(5),
            Subject = new ClaimsIdentity([new Claim("sub", seller.Auth.User.Id.ToString()), new Claim("tenant_id", seller.Auth.User.TenantId.ToString())]),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(new byte[64]), SecurityAlgorithms.HmacSha256),
        });

        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithTokenAsync(forged));
    }

    [Fact]
    public async Task Unsigned_alg_none_token_is_rejected()
    {
        var seller = await Seller.RegisterAsync(factory);
        var payload = new JsonObject
        {
            ["sub"] = seller.Auth.User.Id.ToString(),
            ["tenant_id"] = seller.Auth.User.TenantId.ToString(),
            ["iss"] = "stocksync",
            ["aud"] = "stocksync-web",
            ["exp"] = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
        };
        var unsigned = $"{Encode("""{"alg":"none","typ":"JWT"}""")}.{Encode(payload.ToJsonString())}.";

        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithTokenAsync(unsigned));
    }

    [Fact]
    public async Task Token_with_tampered_tenant_claim_is_rejected()
    {
        var seller = await Seller.RegisterAsync(factory);
        var parts = seller.Auth.AccessToken.Split('.');
        var payload = JsonNode.Parse(Base64UrlEncoder.Decode(parts[1]))!.AsObject();
        payload["tenant_id"] = Guid.NewGuid().ToString();
        var tampered = $"{parts[0]}.{Encode(payload.ToJsonString())}.{parts[2]}";

        Assert.Equal(HttpStatusCode.Unauthorized, await GetProductsWithTokenAsync(tampered));
    }

    [Fact]
    public async Task Access_token_carries_no_personal_data()
    {
        var seller = await Seller.RegisterAsync(factory);

        var token = new JsonWebToken(seller.Auth.AccessToken);

        Assert.DoesNotContain(token.Claims, c => c.Value.Contains('@', StringComparison.Ordinal));
        Assert.Equal("HS256", token.Alg);
    }

    [Fact]
    public async Task Api_responses_carry_security_headers()
    {
        var seller = await Seller.RegisterAsync(factory);

        var response = await seller.Client.GetAsync("/api/products", Ct);

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("default-src 'none'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/api/products?pageSize=1000")]
    [InlineData("/api/products?page=0")]
    [InlineData("/api/orders?pageSize=0")]
    public async Task Out_of_range_paging_is_rejected(string url)
    {
        var seller = await Seller.RegisterAsync(factory);

        Assert.Equal(HttpStatusCode.BadRequest, (await seller.Client.GetAsync(url, Ct)).StatusCode);
    }

    [Fact]
    public async Task Overlong_search_terms_are_rejected()
    {
        var seller = await Seller.RegisterAsync(factory);

        var response = await seller.Client.GetAsync($"/api/orders?search={new string('a', 101)}", Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Sql_metacharacters_in_search_are_treated_as_text()
    {
        var seller = await Seller.RegisterAsync(factory);
        await seller.CreateProductAsync("SAFE-1", 1);

        var response = await seller.Client.GetAsync("/api/products?search=%27%3B%20DROP%20TABLE%20products%3B--", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await seller.Client.GetAsync("/api/products", Ct)).StatusCode);
    }

    [Fact]
    public async Task Malformed_order_bodies_are_rejected_as_bad_requests()
    {
        var seller = await Seller.RegisterAsync(factory);
        var channel = await seller.ChannelAsync("Manual");

        var nullItems = await seller.PostAsync("/api/orders", new { channelId = channel.Id, customerName = "X", items = (object?)null });
        var nullLine = await seller.PostAsync("/api/orders", new { channelId = channel.Id, customerName = "X", items = new object?[] { null } });

        Assert.Equal(HttpStatusCode.BadRequest, nullItems.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, nullLine.StatusCode);
    }

    [Fact]
    public async Task Error_responses_keep_their_security_headers()
    {
        var seller = await Seller.RegisterAsync(factory);
        using var content = new StringContent("{not json", Encoding.UTF8, "application/json");

        var response = await seller.Client.PostAsync("/api/products", content, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Contains("default-src 'none'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Unknown_routes_require_authentication_and_hide_token_errors()
    {
        var anonymous = await factory.CreateClient().GetAsync("/api/not-a-route", Ct);
        var badToken = factory.CreateClient();
        badToken.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.token");
        var rejected = await badToken.GetAsync("/api/products", Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);
        Assert.DoesNotContain("error_description", rejected.Headers.WwwAuthenticate.ToString(), StringComparison.Ordinal);
    }

    private static string Encode(string json) => Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(json));

    private async Task<HttpStatusCode> GetProductsWithTokenAsync(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (await client.GetAsync("/api/products", Ct)).StatusCode;
    }
}
