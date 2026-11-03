namespace StockSync.Application.Features.Auth;

public sealed record UserDto(Guid Id, string Email, string DisplayName, string Role, Guid TenantId, string StoreName);
