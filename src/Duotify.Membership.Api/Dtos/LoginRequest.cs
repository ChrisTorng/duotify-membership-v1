namespace Duotify.Membership.Api.Dtos;

public class LoginRequest
{
    public string EmailOrNationalId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
