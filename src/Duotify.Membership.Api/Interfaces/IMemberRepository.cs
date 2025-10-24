using Duotify.Membership.Api.Models;

namespace Duotify.Membership.Api.Interfaces;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id);
    Task<Member?> GetByNationalIdAsync(string nationalId);
    Task<Member?> GetByEmailAsync(string email);
    Task<bool> NationalIdExistsAsync(string nationalId);
    Task<Member> CreateAsync(Member member);
    Task<Member> UpdateAsync(Member member);
    Task SaveChangesAsync();
}
