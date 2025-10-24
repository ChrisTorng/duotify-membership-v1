using Duotify.Membership.Api.Dtos;
using Duotify.Membership.Api.Interfaces;
using Duotify.Membership.Api.Models;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace Duotify.Membership.Api.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IEmailService _emailService;
    private readonly ILogger<MemberService> _logger;

    public MemberService(
        IMemberRepository memberRepository,
        IVerificationCodeService verificationCodeService,
        IEmailService emailService,
        ILogger<MemberService> logger)
    {
        _memberRepository = memberRepository;
        _verificationCodeService = verificationCodeService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _memberRepository.NationalIdExistsAsync(request.NationalId))
        {
            _logger.LogWarning("Registration attempt with duplicate NationalId: {NationalId}", request.NationalId);
            throw new InvalidOperationException("NATIONAL_ID_ALREADY_EXISTS");
        }

        var passwordHash = BCrypt.HashPassword(request.Password, workFactor: 12);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            NationalId = request.NationalId,
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            IsEmailVerified = false
        };

        var createdMember = await _memberRepository.CreateAsync(member);
        _logger.LogInformation("Member registered: {MemberId}", createdMember.Id);

        var code = await _verificationCodeService.GenerateCodeAsync(createdMember.Id);
        await _emailService.SendVerificationCodeAsync(createdMember.Email, code, createdMember.Id);

        return new RegisterResponse
        {
            MemberId = createdMember.Id,
            Email = createdMember.Email,
            Message = "註冊成功！驗證碼已發送至您的電子郵件。"
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var member = await _memberRepository.GetByEmailAsync(request.EmailOrNationalId) ??
                     await _memberRepository.GetByNationalIdAsync(request.EmailOrNationalId);

        if (member == null || !BCrypt.Verify(request.Password, member.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed for: {Identifier}", request.EmailOrNationalId);
            throw new InvalidOperationException("INVALID_CREDENTIALS");
        }

        _logger.LogInformation("Member logged in: {MemberId}", member.Id);

        return new LoginResponse
        {
            MemberId = member.Id,
            Email = member.Email,
            Name = member.Name,
            IsEmailVerified = member.IsEmailVerified
        };
    }
}
