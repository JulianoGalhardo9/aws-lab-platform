namespace AuthService.Domain.Entities;
public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    protected RefreshToken() { }

    public RefreshToken(string token, TimeSpan validity, Guid userId)
    {
        Id = Guid.NewGuid();
        Token = token;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.Add(validity);
        UserId = userId;
        IsRevoked = false;
    }
    public void Revoke()
    {
        IsRevoked = true;
    }
}