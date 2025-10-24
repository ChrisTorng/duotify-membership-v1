using Duotify.Membership.Api.Interfaces;
using Duotify.Membership.Api.Models;
using Duotify.Membership.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Duotify.Membership.Api.Tests.Services;

public class AuthorizationServiceTests
{
    private readonly Mock<IMemberRepository> _memberRepositoryMock;
    private readonly Mock<ILogger<AuthorizationService>> _loggerMock;
    private readonly AuthorizationService _service;

    public AuthorizationServiceTests()
    {
        _memberRepositoryMock = new Mock<IMemberRepository>();
        _loggerMock = new Mock<ILogger<AuthorizationService>>();
        _service = new AuthorizationService(_memberRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task IsUserEmailVerifiedAsync_WithVerifiedUser_ReturnsTrue()
    {
        var memberId = Guid.NewGuid();
        var member = new Member
        {
            Id = memberId,
            Email = "test@example.com",
            NationalId = "A123456789",
            Name = "Test",
            PasswordHash = "hash",
            IsEmailVerified = true
        };

        _memberRepositoryMock
            .Setup(r => r.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        var result = await _service.IsUserEmailVerifiedAsync(memberId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserEmailVerifiedAsync_WithUnverifiedUser_ReturnsFalse()
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

        var result = await _service.IsUserEmailVerifiedAsync(memberId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsUserEmailVerifiedAsync_WithNonExistentUser_ReturnsFalse()
    {
        var memberId = Guid.NewGuid();

        _memberRepositoryMock
            .Setup(r => r.GetByIdAsync(memberId))
            .ReturnsAsync((Member?)null);

        var result = await _service.IsUserEmailVerifiedAsync(memberId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessProtectedResourceAsync_WithVerifiedUser_ReturnsTrue()
    {
        var memberId = Guid.NewGuid();
        var member = new Member
        {
            Id = memberId,
            Email = "test@example.com",
            NationalId = "A123456789",
            Name = "Test",
            PasswordHash = "hash",
            IsEmailVerified = true
        };

        _memberRepositoryMock
            .Setup(r => r.GetByIdAsync(memberId))
            .ReturnsAsync(member);

        var result = await _service.CanAccessProtectedResourceAsync(memberId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessProtectedResourceAsync_WithUnverifiedUser_ReturnsFalse()
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

        var result = await _service.CanAccessProtectedResourceAsync(memberId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessProtectedResourceAsync_WithNonExistentUser_ReturnsFalse()
    {
        var memberId = Guid.NewGuid();

        _memberRepositoryMock
            .Setup(r => r.GetByIdAsync(memberId))
            .ReturnsAsync((Member?)null);

        var result = await _service.CanAccessProtectedResourceAsync(memberId);

        result.Should().BeFalse();
    }
}
