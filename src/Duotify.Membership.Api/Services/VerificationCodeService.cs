using Duotify.Membership.Api.Data;
using Duotify.Membership.Api.Interfaces;
using Duotify.Membership.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Duotify.Membership.Api.Services;

public class VerificationCodeService : IVerificationCodeService
{
    private readonly IVerificationCodeRepository _repository;
    private readonly IMemberRepository _memberRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<VerificationCodeService> _logger;
    private const int CodeExpirationMinutes = 5;
    private const int MaxFailedAttempts = 3;

    public VerificationCodeService(
        IVerificationCodeRepository repository,
        IMemberRepository memberRepository,
        IEmailService emailService,
        ILogger<VerificationCodeService> logger)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> GenerateCodeAsync(Guid memberId)
    {
        var code = new Random().Next(100000, 999999).ToString();
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(CodeExpirationMinutes),
            IsUsed = false,
            FailedAttempts = 0
        };

        await _repository.CreateAsync(verificationCode);
        _logger.LogInformation("Verification code generated for member: {MemberId}", memberId);
        return code;
    }

    public async Task VerifyCodeAsync(Guid memberId, string code)
    {
        var verificationCode = await _repository.GetActiveCodeByMemberIdAsync(memberId);

        if (verificationCode == null)
        {
            _logger.LogWarning("No active verification code found for member: {MemberId}", memberId);
            throw new InvalidOperationException("NO_ACTIVE_CODE");
        }

        if (verificationCode.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Verification code expired for member: {MemberId}", memberId);
            throw new InvalidOperationException("CODE_EXPIRED");
        }

        if (verificationCode.FailedAttempts >= MaxFailedAttempts)
        {
            _logger.LogWarning("Too many failed attempts for member: {MemberId}", memberId);
            throw new InvalidOperationException("TOO_MANY_ATTEMPTS");
        }

        if (verificationCode.Code != code)
        {
            verificationCode.FailedAttempts++;
            await _repository.UpdateAsync(verificationCode);
            _logger.LogWarning("Invalid code attempt for member: {MemberId}, attempts: {Attempts}", memberId, verificationCode.FailedAttempts);
            throw new InvalidOperationException("INVALID_CODE");
        }

        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member != null)
        {
            member.IsEmailVerified = true;
            await _memberRepository.UpdateAsync(member);
        }

        verificationCode.IsUsed = true;
        await _repository.UpdateAsync(verificationCode);
        _logger.LogInformation("Email verified for member: {MemberId}", memberId);
    }

    public async Task ResendCodeAsync(Guid memberId)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null)
        {
            throw new InvalidOperationException("MEMBER_NOT_FOUND");
        }

        if (member.IsEmailVerified)
        {
            throw new InvalidOperationException("ALREADY_VERIFIED");
        }

        var code = await GenerateCodeAsync(memberId);
        await _emailService.SendVerificationCodeAsync(member.Email, code, memberId);
        _logger.LogInformation("Verification code resent for member: {MemberId}", memberId);
    }
}
