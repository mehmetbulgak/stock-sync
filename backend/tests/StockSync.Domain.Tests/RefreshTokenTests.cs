using StockSync.Domain.Users;

namespace StockSync.Domain.Tests;

public class RefreshTokenTests
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(30);
    private static readonly TimeSpan Grace = TimeSpan.FromSeconds(30);

    [Fact]
    public void New_token_is_active_until_it_expires()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", TestData.Now, Lifetime, SessionLifetime);

        Assert.True(token.IsActive(TestData.Now));
        Assert.False(token.IsActive(TestData.Now.Add(Lifetime)));
    }

    [Fact]
    public void Rotated_token_is_inside_the_grace_window_only_briefly()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", TestData.Now, Lifetime, SessionLifetime);

        token.Revoke(TestData.Now, replacedByTokenId: Guid.NewGuid());

        Assert.False(token.IsActive(TestData.Now));
        Assert.True(token.WasRotatedWithin(Grace, TestData.Now.AddSeconds(10)));
        Assert.False(token.WasRotatedWithin(Grace, TestData.Now.AddMinutes(1)));
    }

    [Fact]
    public void Token_revoked_without_rotation_never_gets_a_grace_window()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", TestData.Now, Lifetime, SessionLifetime);

        token.Revoke(TestData.Now);

        Assert.False(token.WasRotatedWithin(Grace, TestData.Now));
    }

    [Fact]
    public void Revoking_twice_keeps_the_first_revocation()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", TestData.Now, Lifetime, SessionLifetime);
        var replacement = Guid.NewGuid();

        token.Revoke(TestData.Now, replacement);
        token.Revoke(TestData.Now.AddHours(1));

        Assert.Equal(TestData.Now, token.RevokedAt);
        Assert.Equal(replacement, token.ReplacedByTokenId);
    }

    [Fact]
    public void Successor_keeps_the_session_start_and_never_outlives_the_session()
    {
        var first = RefreshToken.Issue(Guid.NewGuid(), "hash-1", TestData.Now, Lifetime, SessionLifetime);
        var late = TestData.Now.AddDays(27);

        var successor = first.Successor("hash-2", late, Lifetime, SessionLifetime);

        Assert.Equal(first.UserId, successor.UserId);
        Assert.Equal(TestData.Now, successor.SessionStartedAt);
        Assert.Equal(TestData.Now.Add(SessionLifetime), successor.ExpiresAt);
        Assert.False(successor.IsActive(TestData.Now.Add(SessionLifetime)));
    }

    [Fact]
    public void Session_lifetime_shorter_than_token_lifetime_caps_the_first_token()
    {
        var token = RefreshToken.Issue(Guid.NewGuid(), "hash", TestData.Now, Lifetime, TimeSpan.FromDays(1));

        Assert.Equal(TestData.Now.AddDays(1), token.ExpiresAt);
    }
}
