using Duotify.Membership.Api.Interfaces;
using Microsoft.Extensions.Logging;

namespace Duotify.Membership.Api.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IMemberRepository _memberRepository;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(IMemberRepository memberRepository, ILogger<AuthorizationService> logger)
    {
        _memberRepository = memberRepository;
        _logger = logger;
    }

    public async Task<bool> IsUserEmailVerifiedAsync(Guid memberId)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null)
        {
            _logger.LogWarning("Authorization check: Member not found. MemberId: {MemberId}", memberId);
            return false;
        }

        return member.IsEmailVerified;
    }

    public async Task<bool> CanAccessProtectedResourceAsync(Guid memberId)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null)
        {
            _logger.LogWarning("Protected resource access: Member not found. MemberId: {MemberId}", memberId);
            return false;
        }

        if (!member.IsEmailVerified)
        {
            _logger.LogWarning("Protected resource access: Email not verified. MemberId: {MemberId}", memberId);
            return false;
        }

        return true;
    }
}
