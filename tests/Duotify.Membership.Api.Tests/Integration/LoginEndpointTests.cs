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
    private static int _testCounter = 0;
    private static readonly string[] ValidTestIds = new[]
    {
        "F100000110", "F100000160", "F100000330", "F100000380", "F100000500", 
        "F100000550", "F100000720", "F100000770", "F100000940", "F100000990"
    };

    public LoginEndpointTests(ApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClientWithTestMode();
    }

    private string GetNextTestId()
    {
        var counter = Interlocked.Increment(ref _testCounter);
        return ValidTestIds[counter % ValidTestIds.Length];
    }

    private async Task<Guid> RegisterMemberAsync(string nationalId, string email)
    {
        var request = new RegisterRequest
        {
            NationalId = nationalId,
            Name = "Login Test User",
            Email = email,
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/register", content);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseBody);
        jsonDocument.RootElement.TryGetProperty("data", out var dataElement).Should().BeTrue();
        dataElement.TryGetProperty("memberId", out var memberIdElement).Should().BeTrue();

        return Guid.Parse(memberIdElement.GetString() ?? "");
    }

    [Fact]
    public async Task Login_WithValidEmail_Returns200()
    {
        var nationalId = GetNextTestId();
        var email = $"logintest{nationalId}@example.com";
        var memberId = await RegisterMemberAsync(nationalId, email);

        var request = new LoginRequest
        {
            EmailOrNationalId = email,
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseBody);
        jsonDocument.RootElement.TryGetProperty("data", out var dataElement).Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithValidNationalId_Returns200()
    {
        var nationalId = GetNextTestId();
        var email = $"logintest{nationalId}@example.com";
        var memberId = await RegisterMemberAsync(nationalId, email);

        var request = new LoginRequest
        {
            EmailOrNationalId = nationalId,
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
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var nationalId = GetNextTestId();
        var email = $"logintest{nationalId}@example.com";
        await RegisterMemberAsync(nationalId, email);

        var request = new LoginRequest
        {
            EmailOrNationalId = email,
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
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
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

    [Fact]
    public async Task Login_UnverifiedUser_CanStillLogin()
    {
        var nationalId = GetNextTestId();
        var email = $"logintest{nationalId}@example.com";
        var memberId = await RegisterMemberAsync(nationalId, email);

        var request = new LoginRequest
        {
            EmailOrNationalId = email,
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
