using PhanMemQuanLyCongNo.Domain.Entities;

namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface IKhachHangRepository
{
    Task<IReadOnlyCollection<KhachHang>> GetAllAsync(Guid tenantId);

    Task<KhachHang?> GetByIdAsync(Guid tenantId, Guid id);

    Task AddAsync(KhachHang khachHang);

    Task UpdateAsync(KhachHang khachHang);

    Task DeleteAsync(KhachHang khachHang);
}