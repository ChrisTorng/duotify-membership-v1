namespace Duotify.Membership.Api.Dtos;

public class VerificationStatusResponse
{
    public Guid MemberId { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool CanAccessProtectedResources { get; set; }
}
