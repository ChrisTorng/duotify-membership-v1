namespace Duotify.Membership.Api.Dtos;

public class LoginResponse
{
    public Guid MemberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}
