using Duotify.Membership.Api.Models;

namespace Duotify.Membership.Api.Interfaces;

public interface IVerificationCodeRepository
{
    Task<VerificationCode?> GetByIdAsync(Guid id);
    Task<VerificationCode?> GetActiveCodeByMemberIdAsync(Guid memberId);
    Task<VerificationCode> CreateAsync(VerificationCode code);
    Task<VerificationCode> UpdateAsync(VerificationCode code);
    Task SaveChangesAsync();
}
