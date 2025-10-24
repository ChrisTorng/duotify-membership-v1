using Duotify.Membership.Api.Interfaces;
using Microsoft.Extensions.Logging;

namespace Duotify.Membership.Api.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendVerificationCodeAsync(string email, string code, Guid memberId)
    {
        _logger.LogInformation("Sending verification code to {Email} for member {MemberId}", email, memberId);
        
        await Task.Delay(100);
        _logger.LogInformation("Verification code email sent to {Email}", email);
    }
}
