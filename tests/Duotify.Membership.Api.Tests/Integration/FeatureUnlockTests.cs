using System.Net;
using System.Text;
using System.Text.Json;
using Duotify.Membership.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace Duotify.Membership.Api.Tests.Integration;

/// <summary>
/// T045: Integration test for feature unlock after email verification
/// </summary>
public class FeatureUnlockTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _httpClient;
    private static int _testCounter = 0;

    public FeatureUnlockTests(ApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    private async Task<(Guid MemberId, string Code)> RegisterAndGetVerificationCodeAsync(string nationalId, string email)
    {
        var request = new RegisterRequest
        {
            NationalId = nationalId,
            Name = "Test User",
            Email = email,
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/register", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseBody);
        jsonDocument.RootElement.TryGetProperty("data", out var dataElement).Should().BeTrue();
        dataElement.TryGetProperty("memberId", out var memberIdElement).Should().BeTrue();
        dataElement.TryGetProperty("verificationCode", out var codeElement).Should().BeTrue();

        var memberId = Guid.Parse(memberIdElement.GetString() ?? "");
        var code = codeElement.GetString() ?? "";

        return (memberId, code);
    }

    [Fact]
    public async Task AfterVerification_UnverifiedUserBecomesFullyVerified()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var (memberId, code) = await RegisterAndGetVerificationCodeAsync($"F100000047", $"verify@example.com");

        // Verify the email
        var verifyRequest = new VerifyCodeRequest { Code = code };
        var verifyContent = new StringContent(
            JsonSerializer.Serialize(verifyRequest),
            Encoding.UTF8,
            "application/json");

        var verifyResponse = await _httpClient.PostAsync($"/v1/members/{memberId}/verify", verifyContent);
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Check profile still accessible
        var profileResponse = await _httpClient.GetAsync($"/v1/members/{memberId}/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AfterVerification_UserCanNotResendCode()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var (memberId, code) = await RegisterAndGetVerificationCodeAsync($"F100000047", $"verify@example.com");

        // Verify the email
        var verifyRequest = new VerifyCodeRequest { Code = code };
        var verifyContent = new StringContent(
            JsonSerializer.Serialize(verifyRequest),
            Encoding.UTF8,
            "application/json");

        var verifyResponse = await _httpClient.PostAsync($"/v1/members/{memberId}/verify", verifyContent);
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Try to resend code (should fail with ALREADY_VERIFIED)
        var resendResponse = await _httpClient.PostAsync($"/v1/members/{memberId}/verification-code/resend", null);
        resendResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var resendBody = await resendResponse.Content.ReadAsStringAsync();
        resendBody.Should().Contain("ALREADY_VERIFIED");
    }

    [Fact]
    public async Task AfterVerification_LoginStillWorks()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var (memberId, code) = await RegisterAndGetVerificationCodeAsync($"F100000047", $"verify@example.com");

        // Verify the email
        var verifyRequest = new VerifyCodeRequest { Code = code };
        var verifyContent = new StringContent(
            JsonSerializer.Serialize(verifyRequest),
            Encoding.UTF8,
            "application/json");

        var verifyResponse = await _httpClient.PostAsync($"/v1/members/{memberId}/verify", verifyContent);
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Login after verification should still work
        var loginRequest = new LoginRequest
        {
            EmailOrNationalId = $"verify@example.com",
            Password = "SecurePassword123!"
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var loginResponse = await _httpClient.PostAsync("/v1/members/login", loginContent);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Verification_WithoutRelogin_UnlocksFeatures()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var (memberId, code) = await RegisterAndGetVerificationCodeAsync($"F100000047", $"verify@example.com");

        // Verify the email
        var verifyRequest = new VerifyCodeRequest { Code = code };
        var verifyContent = new StringContent(
            JsonSerializer.Serialize(verifyRequest),
            Encoding.UTF8,
            "application/json");

        var verifyResponse = await _httpClient.PostAsync($"/v1/members/{memberId}/verify", verifyContent);
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Profile should still be accessible without re-login
        var profileResponse = await _httpClient.GetAsync($"/v1/members/{memberId}/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
