using PhanMemQuanLyCongNo.Domain.Entities;

namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface IThanhToanRepository
{
    Task<IReadOnlyCollection<ThanhToan>> GetAllAsync(Guid tenantId);

    Task<IReadOnlyCollection<ThanhToan>> GetByDebtIdAsync(Guid tenantId, Guid debtId);

    Task<ThanhToan?> GetByIdAsync(Guid tenantId, Guid id);

    Task AddAsync(ThanhToan thanhToan);
}