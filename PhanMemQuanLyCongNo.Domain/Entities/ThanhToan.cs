using PhanMemQuanLyCongNo.Domain.Enums;

namespace PhanMemQuanLyCongNo.Domain.Entities;

public class ThanhToan
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid DebtId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = PhuongThucThanhToan.Cash;
    public DateTime PaidAt { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
}
