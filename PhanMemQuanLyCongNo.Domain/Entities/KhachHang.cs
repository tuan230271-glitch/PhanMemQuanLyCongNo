using PhanMemQuanLyCongNo.Domain.Enums;

namespace PhanMemQuanLyCongNo.Domain.Entities;

public class KhachHang
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CitizenId { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string TrangThai { get; set; } = TrangThaiKhachHang.DangHoatDong;
}
