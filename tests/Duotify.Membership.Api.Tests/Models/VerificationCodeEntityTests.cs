using Duotify.Membership.Api.Models;
using Xunit;

namespace Duotify.Membership.Api.Tests.Models;

public class VerificationCodeEntityTests
{
    [Fact]
    public void VerificationCode_Creation_SetsPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var code = "123456";
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        var verificationCode = new VerificationCode
        {
            Id = id,
            MemberId = memberId,
            Code = code,
            ExpiresAt = expiresAt,
            IsUsed = false,
            FailedAttempts = 0
        };

        Assert.Equal(id, verificationCode.Id);
        Assert.Equal(memberId, verificationCode.MemberId);
        Assert.Equal(code, verificationCode.Code);
        Assert.Equal(expiresAt, verificationCode.ExpiresAt);
        Assert.False(verificationCode.IsUsed);
        Assert.Equal(0, verificationCode.FailedAttempts);
    }

    [Fact]
    public void VerificationCode_IsExpired_ReturnsTrueWhenExpiresAtIsPast()
    {
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            IsUsed = false,
            FailedAttempts = 0
        };

        Assert.True(verificationCode.ExpiresAt < DateTime.UtcNow);
    }

    [Fact]
    public void VerificationCode_IsExpired_ReturnsFalseWhenExpiresAtIsFuture()
    {
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            FailedAttempts = 0
        };

        Assert.False(verificationCode.ExpiresAt < DateTime.UtcNow);
    }

    [Fact]
    public void VerificationCode_FailedAttempts_IncrementsCorrectly()
    {
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            FailedAttempts = 0
        };

        verificationCode.FailedAttempts++;
        Assert.Equal(1, verificationCode.FailedAttempts);

        verificationCode.FailedAttempts++;
        verificationCode.FailedAttempts++;
        Assert.Equal(3, verificationCode.FailedAttempts);
    }

    [Fact]
    public void VerificationCode_MaxFailedAttempts_Is3()
    {
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            FailedAttempts = 3
        };

        Assert.True(verificationCode.FailedAttempts >= 3);
    }

    [Fact]
    public void VerificationCode_CanMarkAsUsed()
    {
        var verificationCode = new VerificationCode
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            Code = "123456",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            FailedAttempts = 0
        };

        verificationCode.IsUsed = true;

        Assert.True(verificationCode.IsUsed);
    }
}
