namespace Duotify.Membership.Api.Interfaces;

public interface IAuthorizationService
{
    Task<bool> IsUserEmailVerifiedAsync(Guid memberId);
    Task<bool> CanAccessProtectedResourceAsync(Guid memberId);
}
