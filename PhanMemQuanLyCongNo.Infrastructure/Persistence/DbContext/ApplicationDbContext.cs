using Microsoft.EntityFrameworkCore;
using PhanMemQuanLyCongNo.Application.Models;
using PhanMemQuanLyCongNo.Domain.Entities;
using DomainContract = PhanMemQuanLyCongNo.Domain.Entities.Contract;

namespace PhanMemQuanLyCongNo.Infrastructure.Persistence.DbContext;

public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<DomainContract> Contracts => Set<DomainContract>();
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<CongNo> CongNos => Set<CongNo>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>().ToTable("Tenants");
        modelBuilder.Entity<DomainContract>().ToTable("Contracts");
        modelBuilder.Entity<AppUser>().ToTable("Users");

        modelBuilder.Entity<KhachHang>().ToTable("Customers");
        modelBuilder.Entity<CongNo>().ToTable("Debts");
        modelBuilder.Entity<ThanhToan>().ToTable("Payments");

        modelBuilder.Entity<DomainContract>(entity =>
        {
            entity.Property(c => c.Amount).HasPrecision(18, 2);
            entity.Property(c => c.InterestRate).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CongNo>(entity =>
        {
            entity.Property(d => d.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(d => d.PenaltyRate).HasPrecision(18, 2);
            entity.Property(d => d.ReminderFee).HasPrecision(18, 2);
            entity.Property(d => d.PaidAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ThanhToan>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);
    }
}
