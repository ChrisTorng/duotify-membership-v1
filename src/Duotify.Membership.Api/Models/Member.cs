namespace Duotify.Membership.Api.Models;

public class Member
{
    public Guid Id { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<VerificationCode>? VerificationCodes { get; set; }
}
