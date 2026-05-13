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
}