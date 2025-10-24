using Duotify.Membership.Api.Interfaces;
using Duotify.Membership.Api.Models;
using Duotify.Membership.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Duotify.Membership.Api.Tests.Services;

public class VerificationCodeServiceTests
{
    private readonly Mock<IVerificationCodeRepository> _repositoryMock;
    private readonly Mock<IMemberRepository> _memberRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<VerificationCodeService>> _loggerMock;
    private readonly VerificationCodeService _service;

    public VerificationCodeServiceTests()
    {
        _repositoryMock = new Mock<IVerificationCodeRepository>();
        _memberRepositoryMock = new Mock<IMemberRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<VerificationCodeService>>();
        _service = new VerificationCodeService(
            _repositoryMock.Object,
            _memberRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GenerateCodeAsync_CreatesNewCode_With6Digits()
    {
        var memberId = Guid.NewGuid();
        VerificationCode? savedCode = null;

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<VerificationCode>()))
            .Callback<VerificationCode>(c => savedCode = c)
            .ReturnsAsync((VerificationCode c) => { c.Id = Guid.NewGuid(); return c; });

        var result = await _service.GenerateCodeAsync(memberId);

        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(6);
        result.All(c => char.IsDigit(c)).Should().BeTrue();
        
        savedCode.Should().NotBeNull();
        savedCode!.MemberId.Should().Be(memberId);
        savedCode.Code.Should().Be(result);
    }

    [Fact]
    public async Task GenerateCodeAsync_ExpiresIn5Minutes()
    {
        var memberId = Guid.NewGuid();
        VerificationCode? savedCode = null;

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<VerificationCode>()))
            .Callback<VerificationCode>(c => savedCode = c)
            .ReturnsAsync((VerificationCode c) => { c.Id = Guid.NewGuid(); return c; });

        await _service.GenerateCodeAsync(memberId);

        savedCode.Should().NotBeNull();
        var expiresIn = savedCode!.ExpiresAt - DateTime.UtcNow;
        expiresIn.Should().BeLessThanOrEqualTo(TimeSpan.FromMinutes(5));
        expiresIn.Should().BeGreaterThan(TimeSpan.FromMinutes(4.9));
    }

    [Fact]
    public async Task VerifyCodeAsync_WithValidCode_MarksAsUsed()
    {
        var memberId = Guid.NewGuid();
        var code = "123456";
        var member = new Member
        {
            Id = memberId,
            Email = "test@example.com",
            NationalId = "A123456789",
            Name = "Test",
            PasswordHash = "hash",
            IsEmailVerified = false
        };

        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            FailedAttempts = 0
        };

        _repositoryMock
            .Setup(r => r.GetActiveCodeByMemberIdAsync(memberId))
            .ReturnsAsync(verificationCode);

        _memberRepositoryMock
            .Setup(r => r.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<VerificationCode>()))
            .ReturnsAsync((VerificationCode c) => c);

        _memberRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Member>()))
            .ReturnsAsync((Member m) => m);

        await _service.VerifyCodeAsync(memberId, code);

        verificationCode.IsUsed.Should().BeTrue();
        member.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyCodeAsync_WithExpiredCode_ThrowsInvalidOperationException()
    {
        var memberId = Guid.NewGuid();
        var code = "123456";
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            IsUsed = false,
            FailedAttempts = 0
        };

        _repositoryMock
            .Setup(r => r.GetActiveCodeByMemberIdAsync(memberId))
            .ReturnsAsync(verificationCode);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.VerifyCodeAsync(memberId, code)
        );
    }

    [Fact]
    public async Task VerifyCodeAsync_WithWrongCode_IncrementsFailedAttempts()
    {
        var memberId = Guid.NewGuid();
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            FailedAttempts = 0
        };

        _repositoryMock
            .Setup(r => r.GetActiveCodeByMemberIdAsync(memberId))
            .ReturnsAsync(verificationCode);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<VerificationCode>()))
            .ReturnsAsync((VerificationCode c) => c);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.VerifyCodeAsync(memberId, "654321")
        );

        verificationCode.FailedAttempts.Should().Be(1);
    }

    [Fact]
    public async Task VerifyCodeAsync_With3FailedAttempts_ThrowsException()
    {
        var memberId = Guid.NewGuid();
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            FailedAttempts = 3
        };

        _repositoryMock
            .Setup(r => r.GetActiveCodeByMemberIdAsync(memberId))
            .ReturnsAsync(verificationCode);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.VerifyCodeAsync(memberId, "123456")
        );
    }

    [Fact]
    public async Task ResendCodeAsync_CreatesNewCode()
    {
        var memberId = Guid.NewGuid();
        var member = new Member
        {
            Id = memberId,
            Email = "test@example.com",
            NationalId = "A123456789",
            Name = "Test",
            PasswordHash = "hash",
            IsEmailVerified = false
        };

        _memberRepositoryMock
            .Setup(r => r.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        VerificationCode? newCode = null;
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<VerificationCode>()))
            .Callback<VerificationCode>(c => newCode = c)
            .ReturnsAsync((VerificationCode c) => { c.Id = Guid.NewGuid(); return c; });

        _emailServiceMock
            .Setup(e => e.SendVerificationCodeAsync(member.Email, It.IsAny<string>(), memberId))
            .Returns(Task.CompletedTask);

        await _service.ResendCodeAsync(memberId);

        newCode.Should().NotBeNull();
        newCode!.MemberId.Should().Be(memberId);
    }

    [Fact]
    public async Task VerifyCodeAsync_WithUsedCode_ThrowsException()
    {
        var memberId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetActiveCodeByMemberIdAsync(memberId))
            .ReturnsAsync((VerificationCode?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.VerifyCodeAsync(memberId, "123456")
        );
    }
}

