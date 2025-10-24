using Duotify.Membership.Api.Dtos;

namespace Duotify.Membership.Api.Interfaces;

public interface IMemberService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
