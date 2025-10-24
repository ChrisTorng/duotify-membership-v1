using System.Net;
using System.Text;
using System.Text.Json;
using Duotify.Membership.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace Duotify.Membership.Api.Tests.Integration;

/// <summary>
/// T044: Integration test for unverified user profile access restrictions
/// </summary>
public class AccessControlTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _httpClient;
    private static int _testCounter = 0;

    public AccessControlTests(ApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    private async Task<Guid> RegisterMemberAsync(string nationalId, string email)
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

        return Guid.Parse(memberIdElement.GetString() ?? "");
    }

    [Fact]
    public async Task UnverifiedUser_CanAccessProfile()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var memberId = await RegisterMemberAsync($"F100000005", $"unverified@example.com");

        var response = await _httpClient.GetAsync($"/v1/members/{memberId}/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnverifiedUser_CanVerifyEmail()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var memberId = await RegisterMemberAsync($"F100000005", $"unverified@example.com");

        var request = new VerifyCodeRequest { Code = "000000" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"/v1/members/{memberId}/verify", content);

        // Should return either 400 (invalid code) or 401 (unauthorized), but not 403 (forbidden)
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnverifiedUser_CanResendCode()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var memberId = await RegisterMemberAsync($"F100000005", $"unverified@example.com");

        var response = await _httpClient.PostAsync($"/v1/members/{memberId}/verification-code/resend", null);

        // Should allow resend
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnverifiedUser_CanLogin()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var memberId = await RegisterMemberAsync($"F100000005", $"unverified@example.com");

        var request = new LoginRequest
        {
            EmailOrNationalId = $"unverified@example.com",
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
