using Duotify.Membership.Api.Dtos;
using Duotify.Membership.Api.Interfaces;
using Duotify.Membership.Api.Models;
using Duotify.Membership.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Duotify.Membership.Api.Tests.Services;

public class MemberServiceTests
{
    private readonly Mock<IMemberRepository> _memberRepositoryMock;
    private readonly Mock<IVerificationCodeService> _verificationCodeServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<MemberService>> _loggerMock;
    private readonly MemberService _memberService;

    public MemberServiceTests()
    {
        _memberRepositoryMock = new Mock<IMemberRepository>();
        _verificationCodeServiceMock = new Mock<IVerificationCodeService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<MemberService>>();

        _memberService = new MemberService(
            _memberRepositoryMock.Object,
            _verificationCodeServiceMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_CreatesAndReturnsResponse()
    {
        var request = new RegisterRequest
        {
            NationalId = "A123456789",
            Name = "John Doe",
            Email = "john@example.com",
            Password = "SecurePassword123!"
        };

        var memberId = Guid.NewGuid();
        var createdMember = new Member
        {
            Id = memberId,
            NationalId = request.NationalId,
            Name = request.Name,
            Email = request.Email,
            PasswordHash = "hashed",
            IsEmailVerified = false
        };

        _memberRepositoryMock
            .Setup(r => r.NationalIdExistsAsync(request.NationalId))
            .ReturnsAsync(false);

        _memberRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Member>()))
            .ReturnsAsync(createdMember);

        _verificationCodeServiceMock
            .Setup(v => v.GenerateCodeAsync(memberId))
            .ReturnsAsync("123456");

        _emailServiceMock
            .Setup(e => e.SendVerificationCodeAsync(request.Email, "123456", memberId))
            .Returns(Task.CompletedTask);

        var result = await _memberService.RegisterAsync(request);

        result.Should().NotBeNull();
        result.MemberId.Should().Be(memberId);
        result.Email.Should().Be(request.Email);
        _memberRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Member>()), Times.Once);
        _verificationCodeServiceMock.Verify(v => v.GenerateCodeAsync(memberId), Times.Once);
        _emailServiceMock.Verify(e => e.SendVerificationCodeAsync(request.Email, "123456", memberId), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateNationalId_ThrowsInvalidOperationException()
    {
        var request = new RegisterRequest
        {
            NationalId = "A123456789",
            Name = "John Doe",
            Email = "john@example.com",
            Password = "SecurePassword123!"
        };

        _memberRepositoryMock
            .Setup(r => r.NationalIdExistsAsync(request.NationalId))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _memberService.RegisterAsync(request)
        );

        _memberRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Member>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        var password = "SecurePassword123!";
        var hashPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            NationalId = "A123456789",
            Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = hashPassword,
            IsEmailVerified = true
        };

        var request = new LoginRequest
        {
            EmailOrNationalId = member.Email,
            Password = password
        };

        _memberRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.EmailOrNationalId))
            .ReturnsAsync(member);

        var result = await _memberService.LoginAsync(request);

        result.Should().NotBeNull();
        result.MemberId.Should().Be(member.Id);
        result.Email.Should().Be(member.Email);
        result.IsEmailVerified.Should().Be(member.IsEmailVerified);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsInvalidOperationException()
    {
        var password = "SecurePassword123!";
        var hashPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            NationalId = "A123456789",
            Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = hashPassword,
            IsEmailVerified = true
        };

        var request = new LoginRequest
        {
            EmailOrNationalId = member.Email,
            Password = "WrongPassword123!"
        };

        _memberRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.EmailOrNationalId))
            .ReturnsAsync(member);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _memberService.LoginAsync(request)
        );
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ThrowsInvalidOperationException()
    {
        var request = new LoginRequest
        {
            EmailOrNationalId = "nonexistent@example.com",
            Password = "Password123!"
        };

        _memberRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.EmailOrNationalId))
            .ReturnsAsync((Member?)null);

        _memberRepositoryMock
            .Setup(r => r.GetByNationalIdAsync(request.EmailOrNationalId))
            .ReturnsAsync((Member?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _memberService.LoginAsync(request)
        );
    }

    [Fact]
    public async Task LoginAsync_WithNationalId_ReturnsLoginResponse()
    {
        var password = "SecurePassword123!";
        var hashPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            NationalId = "A123456789",
            Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = hashPassword,
            IsEmailVerified = false
        };

        var request = new LoginRequest
        {
            EmailOrNationalId = member.NationalId,
            Password = password
        };

        _memberRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.EmailOrNationalId))
            .ReturnsAsync((Member?)null);

        _memberRepositoryMock
            .Setup(r => r.GetByNationalIdAsync(request.EmailOrNationalId))
            .ReturnsAsync(member);

        var result = await _memberService.LoginAsync(request);

        result.Should().NotBeNull();
        result.MemberId.Should().Be(member.Id);
        result.Email.Should().Be(member.Email);
    }
}
