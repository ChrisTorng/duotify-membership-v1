using Duotify.Membership.Api.Validators;
using Xunit;

namespace Duotify.Membership.Api.Tests.Validators;

public class TaiwaneseNationalIdValidatorTests
{
    private readonly TaiwaneseNationalIdValidator _validator = new();

    [Theory]
    [InlineData("A123456789")]
    [InlineData("A123456789")]
    public void IsValid_WithValidId_ReturnsTrue(string nationalId)
    {
        var result = _validator.IsValid(nationalId);
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("ABCDEFGHIJ")]
    [InlineData("12345")]
    public void IsValid_WithInvalidFormat_ReturnsFalse(string nationalId)
    {
        var result = _validator.IsValid(nationalId);
        Assert.False(result);
    }
}
