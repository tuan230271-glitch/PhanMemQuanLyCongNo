namespace PhanMemQuanLyCongNo.Domain.Entities;

public class PaymentReceipt
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PaymentId { get; set; }

    public string ReceiptCode { get; set; } = "";

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public string FilePath { get; set; } = "";
}