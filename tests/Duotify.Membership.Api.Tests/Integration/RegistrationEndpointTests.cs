using System.Net;
using System.Text;
using System.Text.Json;
using Duotify.Membership.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace Duotify.Membership.Api.Tests.Integration;

public class RegistrationEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public RegistrationEndpointTests(ApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidRequest_Returns201Created()
    {
        var request = new RegisterRequest
        {
            NationalId = "A123456789",
            Name = "John Doe",
            Email = "john@example.com",
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
    }

    [Fact]
    public async Task Register_WithDuplicateNationalId_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            NationalId = "B234567890",
            Name = "John Doe",
            Email = "john@example.com",
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        await _httpClient.PostAsync("/v1/members/register", content);

        var duplicateRequest = new RegisterRequest
        {
            NationalId = "B234567890",
            Name = "Jane Doe",
            Email = "jane@example.com",
            Password = "SecurePassword123!"
        };

        var duplicateContent = new StringContent(
            JsonSerializer.Serialize(duplicateRequest),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/register", duplicateContent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidPassword_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            NationalId = "C345678901",
            Name = "John Doe",
            Email = "john@example.com",
            Password = "weak"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            NationalId = "D456789012",
            Name = "John Doe",
            Email = "invalid-email",
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidNationalId_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            NationalId = "INVALID123",
            Name = "John Doe",
            Email = "john@example.com",
            Password = "SecurePassword123!"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/v1/members/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
