using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StockSync.Api.Endpoints;

namespace StockSync.Api.IntegrationTests.Infrastructure;

public sealed class Seller
{
    public const string Password = "Passw0rd!";

    private Seller(HttpClient client, AuthResponse auth, string refreshToken)
    {
        Client = client;
        Auth = auth;
        RefreshToken = refreshToken;
    }

    public HttpClient Client { get; }

    public AuthResponse Auth { get; }

    public string RefreshToken { get; }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static async Task<Seller> RegisterAsync(ApiFactory factory, string? email = null)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                storeName = "Test Store",
                displayName = "Test Owner",
                email = email ?? $"owner-{Guid.NewGuid():N}@test.dev",
                password = Password,
            },
            Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await response.ReadAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return new Seller(client, auth, ReadRefreshCookie(response)!);
    }

    public static string? ReadRefreshCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return null;
        }

        var prefix = $"{RefreshTokenCookie.Name}=";
        var cookie = cookies.FirstOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        var value = cookie?.Split(';')[0][prefix.Length..];
        return string.IsNullOrEmpty(value) ? null : value;
    }

    public static async Task<HttpResponseMessage> RefreshAsync(ApiFactory factory, string refreshToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"{RefreshTokenCookie.Name}={refreshToken}");
        return await factory.CreateClient().SendAsync(request, Ct);
    }

    public async Task<T> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url, Ct);
        response.EnsureSuccessStatusCode();
        return await response.ReadAsync<T>();
    }

    public Task<HttpResponseMessage> PostAsync(string url, object? body = null) =>
        Client.PostAsJsonAsync(url, body, TestJson.Options, Ct);
}
