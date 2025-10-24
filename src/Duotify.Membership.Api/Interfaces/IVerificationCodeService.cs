namespace Duotify.Membership.Api.Interfaces;

public interface IVerificationCodeService
{
    Task<string> GenerateCodeAsync(Guid memberId);
    Task VerifyCodeAsync(Guid memberId, string code);
    Task ResendCodeAsync(Guid memberId);
}
