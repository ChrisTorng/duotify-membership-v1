namespace Duotify.Membership.Api.Dtos;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ErrorResponse? Error { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public ErrorResponse? Error { get; set; }
}
