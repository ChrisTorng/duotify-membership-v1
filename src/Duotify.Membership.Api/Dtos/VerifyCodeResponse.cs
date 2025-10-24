namespace Duotify.Membership.Api.Dtos;

public class VerifyCodeResponse
{
    public string Message { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}
