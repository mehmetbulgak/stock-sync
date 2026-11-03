namespace StockSync.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);

    PasswordCheck Verify(string? passwordHash, string password);
}
