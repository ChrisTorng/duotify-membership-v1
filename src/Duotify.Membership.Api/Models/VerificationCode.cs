namespace Duotify.Membership.Api.Models;

public class VerificationCode
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int FailedAttempts { get; set; }

    public Member? Member { get; set; }
}
