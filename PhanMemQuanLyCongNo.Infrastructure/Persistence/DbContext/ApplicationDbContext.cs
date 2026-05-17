using Microsoft.EntityFrameworkCore;
using PhanMemQuanLyCongNo.Application.Models;
using PhanMemQuanLyCongNo.Domain.Entities;

namespace PhanMemQuanLyCongNo.Infrastructure.Persistence.DbContext;

public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<KhachHang> Customers => Set<KhachHang>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<CongNo> Debts => Set<CongNo>();
    public DbSet<ThanhToan> Payments => Set<ThanhToan>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<CongNo> CongNos => Set<CongNo>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<AppUser> Users => Set<AppUser>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<KhachHang>().ToTable("Customers");
        modelBuilder.Entity<CongNo>().ToTable("Debts");
        modelBuilder.Entity<ThanhToan>().ToTable("Payments");
        modelBuilder.Entity<Tenant>().ToTable("Tenants");
        modelBuilder.Entity<Contract>().ToTable("Contracts");
    }
}
