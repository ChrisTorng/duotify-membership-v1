using System.Net;
using System.Text;
using System.Text.Json;
using Duotify.Membership.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace Duotify.Membership.Api.Tests.Integration;

public class LoginEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public LoginEndpointTests(ApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    private async Task<Guid> RegisterMemberAsync()
    {
        var request = new RegisterRequest
        {
            NationalId = "F678901234",
            Name = "Login Test User",
            Email = "logintest@example.com",
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
    public async Task Login_WithValidEmail_Returns200()
    {
        var memberId = await RegisterMemberAsync();

        var request = new LoginRequest
        {
            EmailOrNationalId = "logintest@example.com",
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        await RegisterMemberAsync();

        var request = new LoginRequest
        {
            EmailOrNationalId = "logintest@example.com",
            Password = "WrongPassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            EmailOrNationalId = "nonexistent@example.com",
            Password = "Password123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
