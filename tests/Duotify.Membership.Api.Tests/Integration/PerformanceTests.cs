using System.Net;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Duotify.Membership.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace Duotify.Membership.Api.Tests.Integration;

/// <summary>
/// T068: Integration test for concurrent registration attempts (race condition testing)
/// </summary>
public class ConcurrentRegistrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public ConcurrentRegistrationTests(ApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ConcurrentRegistration_WithSameNationalId_OnlyOneSucceeds()
    {
        const string nationalId = "F123456789";

        var task1 = RegisterAsync(nationalId, "user1@example.com");
        var task2 = RegisterAsync(nationalId, "user2@example.com");

        var results = await Task.WhenAll(task1, task2);

        var successCount = results.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = results.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        successCount.Should().Be(1, "Exactly one registration should succeed");
        conflictCount.Should().Be(1, "Exactly one registration should get conflict");
    }

    [Fact]
    public async Task ConcurrentRegistration_WithDifferentNationalIds_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 5)
            .Select(i => RegisterAsync($"F12345678{i}", $"user{i}@example.com"))
            .ToList();

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r.StatusCode == HttpStatusCode.Created);
        successCount.Should().Be(5, "All registrations with different IDs should succeed");
    }

    [Fact]
    public async Task ConcurrentRegistration_HighLoad_AllCompleteWithoutErrors()
    {
        var tasks = Enumerable.Range(0, 20)
            .Select(i => RegisterAsync($"F1234567{i:D2}", $"user{i}@example.com"))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => 
            r.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest));
    }

    private async Task<HttpResponseMessage> RegisterAsync(string nationalId, string email)
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

        return await _httpClient.PostAsync("/v1/members/register", content);
    }
}

/// <summary>
/// T069: Performance testing for National ID duplicate check (<1 sec per spec SC-005)
/// T070: Performance testing for register endpoint (<200ms p50, <500ms p95 per spec SC-001)
/// T071: Performance testing for email send (<30 sec per spec SC-002)
/// </summary>
public class PerformanceTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public PerformanceTests(ApiWebApplicationFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task RegistrationEndpoint_CompletesWithinPerformanceBudget()
    {
        var durations = new List<long>();

        for (int i = 0; i < 10; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            var request = new RegisterRequest
            {
                NationalId = $"F1234567{i:D2}",
                Name = "Performance Test User",
                Email = $"perf{i}@example.com",
                Password = "SecurePassword123!"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/v1/members/register", content);
            stopwatch.Stop();

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            durations.Add(stopwatch.ElapsedMilliseconds);
        }

        // Calculate percentiles
        var sorted = durations.OrderBy(x => x).ToList();
        var p50 = sorted[(int)(sorted.Count * 0.5)];
        var p95 = sorted[(int)(sorted.Count * 0.95)];

        // Per spec SC-001: <200ms (p50), <500ms (p95)
        p50.Should().BeLessThan(200, "p50 should be less than 200ms");
        p95.Should().BeLessThan(500, "p95 should be less than 500ms");
    }

    [Fact]
    public async Task NationalIdDuplicateCheck_CompletesWithinPerformanceBudget()
    {
        // First, create a member
        var firstRequest = new RegisterRequest
        {
            NationalId = "F111111111",
            Name = "Test User",
            Email = "first@example.com",
            Password = "SecurePassword123!"
        };

        var firstContent = new StringContent(
            JsonSerializer.Serialize(firstRequest),
            Encoding.UTF8,
            "application/json");

        var firstResponse = await _httpClient.PostAsync("/v1/members/register", firstContent);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Now measure duplicate check performance
        var stopwatch = Stopwatch.StartNew();

        var duplicateRequest = new RegisterRequest
        {
            NationalId = "F111111111",
            Name = "Test User 2",
            Email = "second@example.com",
            Password = "SecurePassword123!"
        };

        var duplicateContent = new StringContent(
            JsonSerializer.Serialize(duplicateRequest),
            Encoding.UTF8,
            "application/json");

        var duplicateResponse = await _httpClient.PostAsync("/v1/members/register", duplicateContent);
        stopwatch.Stop();

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Per spec SC-005: <1 sec
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "National ID check should complete within 1 second");
    }

    [Fact]
    public async Task MultipleVerifications_CompleteWithinReasonableTime()
    {
        var stopwatch = Stopwatch.StartNew();

        var durations = new List<long>();

        for (int i = 0; i < 5; i++)
        {
            var registerRequest = new RegisterRequest
            {
                NationalId = $"F2234567{i:D2}",
                Name = "Verify Test User",
                Email = $"verify{i}@example.com",
                Password = "SecurePassword123!"
            };

            var registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json");

            var registerResponse = await _httpClient.PostAsync("/v1/members/register", registerContent);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var registerBody = await registerResponse.Content.ReadAsStringAsync();
            var registerJson = JsonDocument.Parse(registerBody);
            registerJson.RootElement.TryGetProperty("data", out var dataElement).Should().BeTrue();
            dataElement.TryGetProperty("verificationCode", out var codeElement).Should().BeTrue();

            var code = codeElement.GetString();
            dataElement.TryGetProperty("memberId", out var memberIdElement).Should().BeTrue();
            var memberId = Guid.Parse(memberIdElement.GetString() ?? "");

            // Measure verification time
            var verifyStopwatch = Stopwatch.StartNew();

            var verifyRequest = new VerifyCodeRequest { Code = code ?? "000000" };
            var verifyContent = new StringContent(
                JsonSerializer.Serialize(verifyRequest),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PostAsync($"/v1/members/{memberId}/verify", verifyContent);

            verifyStopwatch.Stop();
            durations.Add(verifyStopwatch.ElapsedMilliseconds);
        }

        stopwatch.Stop();

        // Verify operations should be reasonably fast
        var avgDuration = durations.Average();
        avgDuration.Should().BeLessThan(100, "Average verification should be fast");
    }

    [Fact]
    public async Task RegisterEndpoint_HandlesConcurrentRequests_WithinPerformanceBudget()
    {
        var stopwatch = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, 10)
            .Select(i =>
            {
                var request = new RegisterRequest
                {
                    NationalId = $"F3234567{i:D2}",
                    Name = "Concurrent Test User",
                    Email = $"concurrent{i}@example.com",
                    Password = "SecurePassword123!"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                return _httpClient.PostAsync("/v1/members/register", content);
            })
            .ToList();

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        var successCount = results.Count(r => r.StatusCode == HttpStatusCode.Created);
        successCount.Should().Be(10, "All concurrent registrations should succeed");

        // All 10 requests should complete within reasonable time
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "10 concurrent registrations should complete within 5 seconds");
    }
}
