using Duotify.Membership.Api.Data;
using Duotify.Membership.Api.Interfaces;
using Duotify.Membership.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Duotify.Membership.Api.Repositories;

public class VerificationCodeRepository : IVerificationCodeRepository
{
    private readonly MembershipDbContext _context;

    public VerificationCodeRepository(MembershipDbContext context)
    {
        _context = context;
    }

    public async Task<VerificationCode?> GetByIdAsync(Guid id)
    {
        return await _context.VerificationCodes.FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<VerificationCode?> GetActiveCodeByMemberIdAsync(Guid memberId)
    {
        return await _context.VerificationCodes
            .Where(v => v.MemberId == memberId && !v.IsUsed && v.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<VerificationCode> CreateAsync(VerificationCode code)
    {
        code.CreatedAt = DateTime.UtcNow;
        await _context.VerificationCodes.AddAsync(code);
        await _context.SaveChangesAsync();
        return code;
    }

    public async Task<VerificationCode> UpdateAsync(VerificationCode code)
    {
        _context.VerificationCodes.Update(code);
        await _context.SaveChangesAsync();
        return code;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
