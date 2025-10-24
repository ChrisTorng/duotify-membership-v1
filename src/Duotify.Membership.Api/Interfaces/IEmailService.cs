namespace Duotify.Membership.Api.Interfaces;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string email, string code, Guid memberId);
}
