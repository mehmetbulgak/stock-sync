namespace StockSync.Application.Abstractions;

public interface ILoginThrottle
{
    bool IsBlocked(string normalizedEmail);

    void RecordFailure(string normalizedEmail);
}
