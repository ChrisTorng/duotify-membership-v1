using Duotify.Membership.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Duotify.Membership.Api.Data;

public class MembershipDbContext : DbContext
{
    public MembershipDbContext(DbContextOptions<MembershipDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Member entity
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NationalId)
                .IsRequired()
                .HasMaxLength(10);
            entity.HasIndex(e => e.NationalId)
                .IsUnique();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.IsEmailVerified)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasMany(e => e.VerificationCodes)
                .WithOne(v => v.Member)
                .HasForeignKey(v => v.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure VerificationCode entity
        modelBuilder.Entity<VerificationCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(6);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false);

            entity.Property(e => e.FailedAttempts)
                .HasDefaultValue(0);

            entity.HasIndex(e => e.MemberId);
            entity.HasIndex(e => new { e.MemberId, e.IsUsed, e.ExpiresAt });
        });
    }
}
