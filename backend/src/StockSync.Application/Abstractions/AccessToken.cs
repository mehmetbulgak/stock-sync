namespace StockSync.Application.Abstractions;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
