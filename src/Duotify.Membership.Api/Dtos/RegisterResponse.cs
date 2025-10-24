namespace Duotify.Membership.Api.Dtos;

public class RegisterResponse
{
    public Guid MemberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
