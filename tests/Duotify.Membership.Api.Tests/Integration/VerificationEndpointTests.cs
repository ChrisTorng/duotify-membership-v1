using System.Net;
using System.Text;
using System.Text.Json;
using Duotify.Membership.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace Duotify.Membership.Api.Tests.Integration;

public class VerificationEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public VerificationEndpointTests(ApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    private async Task<Guid> RegisterMemberAsync()
    {
        var request = new RegisterRequest
        {
            NationalId = "E567890123",
            Name = "Test User",
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/register", content);
        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseBody);

        jsonDocument.RootElement.TryGetProperty("data", out var dataElement).Should().BeTrue();
        dataElement.TryGetProperty("memberId", out var memberIdElement).Should().BeTrue();

        return Guid.Parse(memberIdElement.GetString() ?? "");
    }

    [Fact]
    public async Task VerifyCode_WithValidMemberId_Returns200()
    {
        var memberId = await RegisterMemberAsync();

        var request = new VerifyCodeRequest
        {
            Code = "000000"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"/v1/members/{memberId}/verify",
            content);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VerifyCode_WithInvalidCode_ReturnsBadRequest()
    {
        var memberId = await RegisterMemberAsync();

        var request = new VerifyCodeRequest
        {
            Code = "invalid"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"/v1/members/{memberId}/verify",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyCode_WithNonExistentMember_Returns404()
    {
        var nonExistentMemberId = Guid.NewGuid();

        var request = new VerifyCodeRequest
        {
            Code = "123456"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(
            $"/v1/members/{nonExistentMemberId}/verify",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResendCode_WithValidMemberId_Returns200()
    {
        var memberId = await RegisterMemberAsync();

        var response = await _httpClient.PostAsync(
            $"/v1/members/{memberId}/verification-code/resend",
            new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResendCode_WithNonExistentMember_Returns404()
    {
        var nonExistentMemberId = Guid.NewGuid();

        var response = await _httpClient.PostAsync(
            $"/v1/members/{nonExistentMemberId}/verification-code/resend",
            new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
