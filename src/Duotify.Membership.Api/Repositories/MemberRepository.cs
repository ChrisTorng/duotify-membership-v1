using Duotify.Membership.Api.Data;
using Duotify.Membership.Api.Interfaces;
using Duotify.Membership.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Duotify.Membership.Api.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly MembershipDbContext _context;

    public MemberRepository(MembershipDbContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByIdAsync(Guid id)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member?> GetByNationalIdAsync(string nationalId)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.NationalId == nationalId);
    }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
    }

    public async Task<bool> NationalIdExistsAsync(string nationalId)
    {
        return await _context.Members.AnyAsync(m => m.NationalId == nationalId);
    }

    public async Task<Member> CreateAsync(Member member)
    {
        member.CreatedAt = DateTime.UtcNow;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<Member> UpdateAsync(Member member)
    {
        member.UpdatedAt = DateTime.UtcNow;
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
