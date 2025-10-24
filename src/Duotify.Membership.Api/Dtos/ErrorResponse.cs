namespace Duotify.Membership.Api.Dtos;

public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? RemainingAttempts { get; set; }
}
