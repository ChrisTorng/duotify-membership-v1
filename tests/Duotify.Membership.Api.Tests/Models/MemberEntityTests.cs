using Duotify.Membership.Api.Models;
using Xunit;

namespace Duotify.Membership.Api.Tests.Models;

public class MemberEntityTests
{
    [Fact]
    public void Member_Creation_SetsPropertiesCorrectly()
    {
        var memberId = Guid.NewGuid();
        var email = "test@example.com";
        var nationalId = "A123456789";
        var name = "John Doe";
        var passwordHash = "hashed_password";

        var member = new Member
        {
            Id = memberId,
            Email = email,
            NationalId = nationalId,
            Name = name,
            PasswordHash = passwordHash,
            IsEmailVerified = false
        };

        Assert.Equal(memberId, member.Id);
        Assert.Equal(email, member.Email);
        Assert.Equal(nationalId, member.NationalId);
        Assert.Equal(name, member.Name);
        Assert.Equal(passwordHash, member.PasswordHash);
        Assert.False(member.IsEmailVerified);
    }

    [Fact]
    public void Member_VerifyEmail_UpdatesFlag()
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NationalId = "A123456789",
            Name = "John Doe",
            PasswordHash = "hash",
            IsEmailVerified = false
        };

        member.IsEmailVerified = true;

        Assert.True(member.IsEmailVerified);
    }

    [Fact]
    public void Member_CreatedAt_IsSet()
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            NationalId = "A123456789",
            Name = "John Doe",
            PasswordHash = "hash"
        };

        var before = DateTime.UtcNow;
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(member.CreatedAt, before, after);
    }
}
