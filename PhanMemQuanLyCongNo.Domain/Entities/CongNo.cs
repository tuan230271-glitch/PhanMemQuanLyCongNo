using PhanMemQuanLyCongNo.Domain.Enums;

namespace PhanMemQuanLyCongNo.Domain.Entities;

public class CongNo
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public Guid ContractId { get; init; }
    public decimal PrincipalAmount { get; init; }
    public decimal PenaltyRate { get; init; }
    public decimal ReminderFee { get; init; }
    public DateOnly IssuedDate { get; init; }
    public DateOnly DueDate { get; init; }
    public string Status { get; set; } = TrangThaiCongNo.Draft;
    public string Note { get; init; } = "";

    public decimal PaidAmount { get; private set; }
    public decimal PenaltyAmount => CalculatePenalty(DateOnly.FromDateTime(DateTime.UtcNow));
    public decimal TotalAmount => PrincipalAmount + PenaltyAmount + ReminderFee;
    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);
    public int OverdueDays => Math.Max(0, DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - DueDate.DayNumber);

    public void ApplyPayment(decimal amount)
    {
        PaidAmount += amount;
        RefreshStatus();
    }

    public void RefreshStatus()
    {
        if (Status is TrangThaiCongNo.Cancelled or TrangThaiCongNo.Closed)
        {
            return;
        }

        if (RemainingAmount <= 0)
        {
            Status = TrangThaiCongNo.Paid;
            return;
        }

        if (PaidAmount > 0)
        {
            Status = TrangThaiCongNo.PartialPaid;
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (DueDate < today)
        {
            Status = TrangThaiCongNo.Overdue;
        }
        else if (DueDate.DayNumber - today.DayNumber <= 7)
        {
            Status = TrangThaiCongNo.DueSoon;
        }
        else
        {
            Status = TrangThaiCongNo.Active;
        }
    }

    private decimal CalculatePenalty(DateOnly today)
    {
        var lateDays = Math.Max(0, today.DayNumber - DueDate.DayNumber);
        if (lateDays == 0 || PenaltyRate <= 0)
        {
            return 0;
        }

        return Math.Round(PrincipalAmount * PenaltyRate / 100m / 30m * lateDays, 0);
    }
}
