using StockSync.Application.Features.Auth;

namespace StockSync.Api.Endpoints;

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, UserDto User);
